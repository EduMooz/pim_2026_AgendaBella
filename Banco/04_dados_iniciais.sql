/* =====================================================================
   AgendaBella  |  Arquivo 04 - DADOS INICIAIS (obrigatórios)

   Insere o mínimo para o sistema funcionar:
     - a profissional (administradora do painel);
     - o catálogo de serviços;
     - a grade de horários de início (de 30 em 30 minutos);
     - o expediente semanal.

   Só insere se a tabela estiver vazia, então pode rodar mais de uma vez.
   Senhas aqui já estão em HASH (PBKDF2). A senha de demonstração da
   profissional é:  Erika@123   (troque antes de usar de verdade!)
   ===================================================================== */

USE AgendaBella;
GO

-- 1) Profissional / administradora ----------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Profissional)
    INSERT INTO dbo.Profissional (nome, email, senha, telefone)
    VALUES (N'Erika Santana', N'erika@agendabella.com.br',
            N'PBKDF2$600000$LyirMykhsQA4UvPPtwjEfQ==$OCjmWLn39U+ZIQtMjn7wcGWX3IXg7aKbwh1J6vGIRTo=',
            N'(16) 99999-9999');
GO

-- 2) Serviços --------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Servico)
    INSERT INTO dbo.Servico (nome, descricao, preco, duracao) VALUES
    (N'Cutilagem e Esmaltação das Mãos', N'Remoção de cutículas, lixamento e esmaltação com acabamento impecável.', 45.00, 45),
    (N'Cutilagem e Esmaltação dos Pés',  N'Cuidado com as cutículas, lixamento e esmaltação para pés bem cuidados.', 50.00, 60),
    (N'Esmaltação em Gel',               N'Esmalte em gel com maior durabilidade e brilho.',                          70.00, 60),
    (N'Alongamento de Unhas',            N'Alongamento em gel ou fibra, com formato e decoração à escolha.',         120.00, 90),
    (N'Remoção do Alongamento',          N'Remoção segura do alongamento, preservando a unha natural.',               40.00, 30),
    (N'Spa dos Pés',                     N'Esfoliação, hidratação profunda e massagem relaxante.',                    90.00, 60),
    (N'Hidratação dos Pés',              N'Hidratação intensiva para pés macios.',                                    40.00, 30),
    (N'Manutenção de Alongamento',       N'Reposição e correção do alongamento já existente.',                        80.00, 60);
GO

-- 3) Grade de horários de início: 09:00, 09:30, ... , 18:30 -----------------
IF NOT EXISTS (SELECT 1 FROM dbo.Horario_Agendamento)
BEGIN
    ;WITH n AS (
        SELECT TOP (20) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS k
        FROM sys.all_objects
    )
    INSERT INTO dbo.Horario_Agendamento (horario)
    SELECT DATEADD(MINUTE, k * 30, CAST('09:00:00' AS TIME(0)))
    FROM n;
END
GO

-- 4) Expediente semanal (1=segunda ... 6=sábado; domingo fechado) ----------
--    Segunda a sexta: 09h-12h e 13h-19h  |  Sábado: 09h-12h e 13h-17h
IF NOT EXISTS (SELECT 1 FROM dbo.Horario_Trabalho)
BEGIN
    DECLARE @prof INT = (SELECT TOP (1) id_profissional FROM dbo.Profissional ORDER BY id_profissional);

    INSERT INTO dbo.Horario_Trabalho (id_profissional, dia_semana, hora_inicio, hora_fim)
    SELECT @prof, d.dia, '09:00', '12:00'
    FROM (VALUES (1),(2),(3),(4),(5),(6)) AS d(dia);

    INSERT INTO dbo.Horario_Trabalho (id_profissional, dia_semana, hora_inicio, hora_fim)
    SELECT @prof, d.dia, '13:00', CASE WHEN d.dia = 6 THEN '17:00' ELSE '19:00' END
    FROM (VALUES (1),(2),(3),(4),(5),(6)) AS d(dia);
END
GO

PRINT N'Dados iniciais inseridos.';
GO
