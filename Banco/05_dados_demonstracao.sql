/* =====================================================================
   AgendaBella  |  Arquivo 05 - DADOS DE DEMONSTRAÇÃO (opcional)

   Cria clientes, ~8 semanas de histórico de atendimentos, agendamentos
   futuros, uma lista de espera e um bloqueio de agenda. Serve para a
   tela de Relatórios e a previsão de demanda terem o que mostrar.

   ATENÇÃO: rode em um banco recém-criado (depois dos arquivos 01 a 04),
   porque o script usa os ids 1..5 (clientes), 1 (profissional) e
   1..8 (serviços), que são os gerados em um banco novo.

   Senha de todos os clientes de demonstração:  Cliente@123
   ===================================================================== */

USE AgendaBella;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Cliente)
BEGIN
    -- 1) Clientes -----------------------------------------------------------
    INSERT INTO dbo.Cliente (nome, email, senha, telefone) VALUES
    (N'Maria Silva',    N'cliente@agendabella.com.br', N'PBKDF2$600000$7cJ653inV9tGSe+OkjST3Q==$H1o56lGpuoILV+FMb6t5cvHfawmxu/FxQsJqPe/r7BY=', N'(16) 98888-1111'),
    (N'Ana Souza',      N'ana.souza@exemplo.com',      N'PBKDF2$600000$mHbO3avSAUHM3sXLjuFKng==$ky9DoaLjYn/9r2kKAMriw8AhJonJ009kDwcc528m68I=', N'(16) 98888-2222'),
    (N'Juliana Costa',  N'juliana.costa@exemplo.com',  N'PBKDF2$600000$wUxMvAiiI5bGksDLkS5Baw==$ulm07RHQxHErp4NPCJkktYn2OnsaacCfSQgVQZ0syiw=', N'(16) 98888-3333'),
    (N'Fernanda Lima',  N'fernanda.lima@exemplo.com',  N'PBKDF2$600000$mkM2sdi1/FGRtY7vOul53A==$+Bb8PLhuLHk95CIyRiqgIlATKRQx/+knBgQLPc/cRv8=', N'(16) 98888-4444'),
    (N'Camila Rocha',   N'camila.rocha@exemplo.com',   N'PBKDF2$600000$qA0mjVAhegOzjp0cNELpgw==$UEGLmwdFe/NWvM32Ruo9L1Fq6ZckF7WEcMXgsJy/tyc=', N'(16) 98888-5555');

    -- 2) Preferências de duas clientes --------------------------------------
    INSERT INTO dbo.Preferencia_Cliente (id_cliente, formato_unha, cor_favorita, estilo, observacao) VALUES
    (1, N'Amendoada', N'Rosa nude',  N'Minimalista',  N'Alergia a esmalte com formol.'),
    (2, N'Quadrada',  N'Vermelho',   N'Clássico',     NULL);

    -- 3) Bônus de boas-vindas (10 pontos para cada cliente) -------------------
    INSERT INTO dbo.Movimentacao_Pontos (id_cliente, quantidade, tipo, descricao)
    SELECT id_cliente, 10, N'Credito', N'Bônus de boas-vindas' FROM dbo.Cliente;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Agendamento)
BEGIN
    DECLARE @hoje DATE = CAST(SYSDATETIME() AS DATE);

    -- 4) Histórico: dias úteis dos últimos 56 dias, com mais movimento nos
    --    dias mais recentes (tendência de crescimento para a previsão).
    ;WITH n AS (
        SELECT TOP (56) CAST(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS INT) AS n
        FROM sys.all_objects
    ),
    dias AS (
        SELECT n, DATEADD(DAY, -n, @hoje) AS d FROM n
    ),
    uteis AS (
        SELECT n, d FROM dias
        WHERE (DATEDIFF(DAY, '19000101', d) % 7) BETWEEN 0 AND 4      -- segunda a sexta
    ),
    slots AS (
        SELECT k, CAST(h AS TIME(0)) AS h
        FROM (VALUES (1,'09:00'), (2,'11:00'), (3,'14:00'), (4,'16:30')) AS v(k, h)
    )
    INSERT INTO dbo.Agendamento (id_cliente, id_profissional, id_servico, data_agendamento, horario, status)
    SELECT 1 + ((u.n * s.k) % 5),
           1,
           1 + ((u.n + s.k) % 8),
           u.d,
           s.h,
           N'Confirmado'
    FROM uteis u
    CROSS JOIN slots s
    WHERE s.k <= 1 + ((56 - u.n) / 20) + (u.n % 2);

    -- Transições de status feitas por UPDATE para as TRIGGERS dispararem
    -- (histórico em Historico_Agendamento e pontos em Movimentacao_Pontos).
    UPDATE dbo.Agendamento SET status = N'Cancelado' WHERE id_agendamento % 7 = 0;
    UPDATE dbo.Agendamento SET status = N'Concluido'
     WHERE status = N'Confirmado' AND data_agendamento < @hoje;

    -- 5) Próximos 3 dias úteis: alguns agendamentos pendentes/confirmados.
    ;WITH n AS (
        SELECT TOP (10) CAST(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS INT) AS n
        FROM sys.all_objects
    ),
    prox AS (
        SELECT d, ROW_NUMBER() OVER (ORDER BY d) AS r
        FROM (SELECT DATEADD(DAY, n, @hoje) AS d FROM n) x
        WHERE (DATEDIFF(DAY, '19000101', d) % 7) BETWEEN 0 AND 4
    )
    INSERT INTO dbo.Agendamento (id_cliente, id_profissional, id_servico, data_agendamento, horario, status, observacao)
    SELECT v.cli, 1, v.srv, p.d, CAST(v.h AS TIME(0)), v.st, v.obs
    FROM prox p
    JOIN (VALUES
        (1, 1, 4, '10:00', N'Pendente',   N'Quero um modelo amendoado, cor nude.'),
        (2, 1, 1, '14:00', N'Confirmado', NULL),
        (3, 2, 3, '09:00', N'Pendente',   NULL),
        (4, 2, 2, '15:00', N'Confirmado', NULL),
        (5, 3, 6, '11:00', N'Pendente',   NULL)
    ) AS v(cli, r, srv, h, st, obs) ON v.r = p.r
    WHERE p.r <= 3;

    -- 6) Lista de espera e bloqueio no 1º dia útil futuro.
    DECLARE @prox1 DATE =
        (SELECT TOP (1) d FROM (SELECT DATEADD(DAY, n, @hoje) AS d
                                FROM (VALUES (1),(2),(3),(4)) AS q(n)) z
         WHERE (DATEDIFF(DAY, '19000101', d) % 7) BETWEEN 0 AND 4 ORDER BY d);

    INSERT INTO dbo.Lista_Espera (id_cliente, id_profissional, id_servico, data_desejada, horario_desejado)
    VALUES (4, 1, 4, @prox1, NULL);

    INSERT INTO dbo.Bloqueio_Horario (id_profissional, data_bloqueio, hora_inicio, hora_fim, motivo)
    VALUES (1, DATEADD(DAY, 7, @prox1), '15:00', '17:00', N'Curso de capacitação');
END
GO

PRINT N'Dados de demonstração inseridos.';
GO
