/* =====================================================================
   AgendaBella  |  Arquivo 02 - TRIGGERS (gatilhos)

   Uma trigger é um bloco de SQL que o próprio banco executa quando uma
   linha é inserida, alterada ou excluída. O sistema (C#) não chama a
   trigger: basta gravar o agendamento que o banco dispara as três abaixo.

   Dentro de uma trigger existem duas tabelas virtuais:
     inserted = como a linha FICOU depois do INSERT/UPDATE
     deleted  = como a linha ERA antes do UPDATE (vazia no INSERT)
   Comparar as duas é como a trigger descobre "o que mudou".

   As três triggers estão na tabela Agendamento. No EF Core, isso é
   declarado em AppDbContext com HasTrigger(...) para cada uma delas.
   ===================================================================== */

USE AgendaBella;
GO

/* ---------------------------------------------------------------------
   TRIGGER 1 - TR_Agendamento_Historico
   Registra na tabela Historico_Agendamento:
     - a criação do agendamento;
     - toda mudança de status;
     - todo reagendamento (mudança de data ou horário).
   --------------------------------------------------------------------- */
CREATE OR ALTER TRIGGER dbo.TR_Agendamento_Historico
ON dbo.Agendamento
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- (a) Criação: a linha existe em "inserted" mas não existe em "deleted".
    INSERT INTO dbo.Historico_Agendamento
        (id_agendamento, tipo_evento, status_anterior, status_novo,
         data_anterior, horario_anterior, data_nova, horario_novo)
    SELECT i.id_agendamento, N'Criacao', NULL, i.status,
           NULL, NULL, i.data_agendamento, i.horario
    FROM inserted i
    LEFT JOIN deleted d ON d.id_agendamento = i.id_agendamento
    WHERE d.id_agendamento IS NULL;

    -- (b) Alteração: existe nas duas tabelas e algo importante mudou.
    INSERT INTO dbo.Historico_Agendamento
        (id_agendamento, tipo_evento, status_anterior, status_novo,
         data_anterior, horario_anterior, data_nova, horario_novo)
    SELECT i.id_agendamento,
           CASE WHEN i.data_agendamento <> d.data_agendamento OR i.horario <> d.horario
                THEN N'Reagendamento' ELSE N'MudancaStatus' END,
           d.status, i.status,
           d.data_agendamento, d.horario,
           i.data_agendamento, i.horario
    FROM inserted i
    JOIN deleted d ON d.id_agendamento = i.id_agendamento
    WHERE i.status <> d.status
       OR i.data_agendamento <> d.data_agendamento
       OR i.horario <> d.horario;
END
GO

/* ---------------------------------------------------------------------
   TRIGGER 2 - TR_Agendamento_Pontos
   Quando um agendamento passa a "Concluido", credita pontos de
   fidelidade para a cliente: 1 ponto para cada R$ 1,00 do serviço.
   A comparação com "deleted" garante que o crédito acontece uma única
   vez (só na mudança para Concluido, e não em qualquer outro UPDATE).
   --------------------------------------------------------------------- */
CREATE OR ALTER TRIGGER dbo.TR_Agendamento_Pontos
ON dbo.Agendamento
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Movimentacao_Pontos (id_cliente, id_agendamento, quantidade, tipo, descricao)
    SELECT i.id_cliente,
           i.id_agendamento,
           CAST(FLOOR(s.preco) AS INT),
           N'Credito',
           N'Pontos pelo atendimento: ' + s.nome
    FROM inserted i
    JOIN deleted d       ON d.id_agendamento = i.id_agendamento
    JOIN dbo.Servico s   ON s.id_servico     = i.id_servico
    WHERE i.status = N'Concluido'
      AND d.status <> N'Concluido'
      AND FLOOR(s.preco) > 0;
END
GO

/* ---------------------------------------------------------------------
   TRIGGER 3 - TR_Agendamento_ListaEspera
   Quando uma vaga é liberada (agendamento cancelado OU reagendado para
   outro dia/horário), a cliente mais antiga da lista de espera daquele
   dia passa de "Aguardando" para "Notificado". O aplicativo e o site
   mostram o aviso "vaga liberada" para ela.

   Só UMA pessoa é notificada por vaga liberada (a primeira da fila).
   --------------------------------------------------------------------- */
CREATE OR ALTER TRIGGER dbo.TR_Agendamento_ListaEspera
ON dbo.Agendamento
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH liberados AS (
        -- Dados do horário ANTIGO (deleted), que ficou vago.
        SELECT d.id_agendamento, d.id_profissional, d.data_agendamento, d.horario
        FROM inserted i
        JOIN deleted d ON d.id_agendamento = i.id_agendamento
        WHERE (i.status = N'Cancelado' AND d.status <> N'Cancelado')
           OR (i.status <> N'Cancelado'
               AND (i.data_agendamento <> d.data_agendamento OR i.horario <> d.horario))
    ),
    escolhidos AS (
        -- Para cada vaga, pega a primeira da fila (a mais antiga).
        SELECT DISTINCT x.id_lista_espera
        FROM liberados v
        CROSS APPLY (
            SELECT TOP (1) l.id_lista_espera
            FROM dbo.Lista_Espera l
            WHERE l.status = N'Aguardando'
              AND l.data_desejada = v.data_agendamento
              AND (l.id_profissional IS NULL OR l.id_profissional = v.id_profissional)
              AND (l.horario_desejado IS NULL OR l.horario_desejado = v.horario)
            ORDER BY l.data_solicitacao, l.id_lista_espera
        ) x
    )
    UPDATE l
       SET l.status = N'Notificado'
    FROM dbo.Lista_Espera l
    JOIN escolhidos e ON e.id_lista_espera = l.id_lista_espera;
END
GO

PRINT N'Triggers do AgendaBella criadas com sucesso.';
GO
