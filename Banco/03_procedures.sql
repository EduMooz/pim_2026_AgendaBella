/* =====================================================================
   AgendaBella  |  Arquivo 03 - STORED PROCEDURES

   Uma stored procedure é uma rotina SQL guardada no banco e chamada pelo
   nome. As quatro abaixo são executadas pelo sistema, na camada
   Repositorio, usando Dapper com CommandType.StoredProcedure:

     sp_Horarios_Disponiveis        -> tela/endpoint de agendamento
     sp_Relatorio_Servicos_Periodo  -> relatório gerencial
     sp_Relatorio_Demanda_Semanal   -> previsão de demanda (regressão linear)
     sp_Relatorio_Demanda_DiaSemana -> dia de maior movimento

   Observação sobre o dia da semana:
     (DATEDIFF(DAY,'19000101', data) % 7) + 1  devolve 1=segunda ... 7=domingo
     em qualquer servidor, porque 01/01/1900 foi uma segunda-feira. Assim o
     resultado não depende do idioma nem do SET DATEFIRST do SQL Server.
   ===================================================================== */

USE AgendaBella;
GO

/* ---------------------------------------------------------------------
   sp_Horarios_Disponiveis
   Devolve os horários de INÍCIO em que um serviço cabe na agenda da
   profissional em uma data. Um horário só é oferecido quando TODAS as
   condições abaixo são verdadeiras:
     1. está dentro do expediente (Horario_Trabalho) daquele dia da semana
        e o serviço TERMINA antes do fim do expediente;
     2. não colide com um bloqueio (Bloqueio_Horario);
     3. não colide com outro agendamento ativo (considera a duração dele);
     4. ainda não passou (no dia de hoje, só horários futuros).

   @IgnorarAgendamento: usado no REAGENDAMENTO, para o agendamento que
   está sendo movido não "bloquear" a si mesmo.
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_Horarios_Disponiveis
    @IdProfissional      INT,
    @Data                DATE,
    @IdServico           INT,
    @IgnorarAgendamento  INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Duracao INT =
        (SELECT duracao FROM dbo.Servico WHERE id_servico = @IdServico AND ativo = 1);

    -- Serviço inexistente ou desativado: nenhum horário.
    IF @Duracao IS NULL
    BEGIN
        SELECT CAST(NULL AS INT) AS IdHorario, CAST(NULL AS TIME(0)) AS Horario WHERE 1 = 0;
        RETURN;
    END

    DECLARE @DiaSemana INT = (DATEDIFF(DAY, '19000101', @Data) % 7) + 1;
    DECLARE @Agora DATETIME2(0) = SYSDATETIME();
    DECLARE @MeiaNoite TIME(0) = '00:00:00';
    DECLARE @FimDoDia  TIME(0) = '23:59:59';

    SELECT h.id_horario AS IdHorario,
           h.horario    AS Horario
    FROM dbo.Horario_Agendamento h
    WHERE
        -- 1) dentro do expediente
        EXISTS (
            SELECT 1
            FROM dbo.Horario_Trabalho t
            WHERE t.id_profissional = @IdProfissional
              AND t.dia_semana      = @DiaSemana
              AND h.horario                            >= t.hora_inicio
              AND DATEADD(MINUTE, @Duracao, h.horario) <= t.hora_fim
              AND DATEADD(MINUTE, @Duracao, h.horario) >  h.horario   -- não "vira" a meia-noite
        )
        -- 2) sem bloqueio
        AND NOT EXISTS (
            SELECT 1
            FROM dbo.Bloqueio_Horario b
            WHERE b.id_profissional = @IdProfissional
              AND b.data_bloqueio   = @Data
              AND COALESCE(b.hora_inicio, @MeiaNoite) < DATEADD(MINUTE, @Duracao, h.horario)
              AND COALESCE(b.hora_fim,    @FimDoDia)  > h.horario
        )
        -- 3) sem outro agendamento ativo sobreposto
        AND NOT EXISTS (
            SELECT 1
            FROM dbo.Agendamento a
            JOIN dbo.Servico sa ON sa.id_servico = a.id_servico
            WHERE a.id_profissional  = @IdProfissional
              AND a.data_agendamento = @Data
              AND a.status <> N'Cancelado'
              AND a.id_agendamento <> COALESCE(@IgnorarAgendamento, 0)
              AND a.horario < DATEADD(MINUTE, @Duracao, h.horario)
              AND DATEADD(MINUTE, sa.duracao, a.horario) > h.horario
        )
        -- 4) horário que ainda não passou
        AND DATEADD(SECOND, DATEDIFF(SECOND, @MeiaNoite, h.horario), CAST(@Data AS DATETIME2(0))) > @Agora
    ORDER BY h.horario;
END
GO

/* ---------------------------------------------------------------------
   sp_Relatorio_Servicos_Periodo
   Uma linha por serviço, com totais do período informado.
   Faturamento considera apenas atendimentos "Concluido".
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_Relatorio_Servicos_Periodo
    @DataInicio DATE,
    @DataFim    DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT s.id_servico AS IdServico,
           s.nome       AS Servico,
           COUNT(a.id_agendamento)                                          AS TotalAgendamentos,
           SUM(CASE WHEN a.status = N'Concluido' THEN 1 ELSE 0 END)         AS Concluidos,
           SUM(CASE WHEN a.status = N'Cancelado' THEN 1 ELSE 0 END)         AS Cancelados,
           CAST(SUM(CASE WHEN a.status = N'Concluido' THEN s.preco ELSE 0 END) AS DECIMAL(12,2)) AS Faturamento
    FROM dbo.Servico s
    LEFT JOIN dbo.Agendamento a
           ON a.id_servico = s.id_servico
          AND a.data_agendamento BETWEEN @DataInicio AND @DataFim
    GROUP BY s.id_servico, s.nome
    ORDER BY Faturamento DESC, TotalAgendamentos DESC, s.nome;
END
GO

/* ---------------------------------------------------------------------
   sp_Relatorio_Demanda_Semanal
   Quantidade de agendamentos (não cancelados) em cada uma das últimas
   @Semanas semanas COMPLETAS (segunda a domingo), da mais antiga para a
   mais recente. Semanas sem movimento voltam com Total = 0.
   É a base da previsão de demanda (regressão linear) no C#.
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_Relatorio_Demanda_Semanal
    @Semanas INT = 8
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Hoje DATE = CAST(SYSDATETIME() AS DATE);
    -- Segunda-feira da semana atual (a semana atual não entra: está incompleta).
    DECLARE @SegundaAtual DATE = DATEADD(DAY, -(DATEDIFF(DAY, '19000101', @Hoje) % 7), @Hoje);

    ;WITH semanas AS (
        SELECT TOP (@Semanas)
               DATEADD(WEEK, -CAST(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS INT), @SegundaAtual) AS inicio
        FROM sys.all_objects
    )
    SELECT s.inicio AS InicioSemana,
           COUNT(a.id_agendamento) AS Total
    FROM semanas s
    LEFT JOIN dbo.Agendamento a
           ON a.data_agendamento >= s.inicio
          AND a.data_agendamento <  DATEADD(WEEK, 1, s.inicio)
          AND a.status <> N'Cancelado'
    GROUP BY s.inicio
    ORDER BY s.inicio;
END
GO

/* ---------------------------------------------------------------------
   sp_Relatorio_Demanda_DiaSemana
   Total de agendamentos (não cancelados) por dia da semana, no período.
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_Relatorio_Demanda_DiaSemana
    @DataInicio DATE,
    @DataFim    DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT (DATEDIFF(DAY, '19000101', a.data_agendamento) % 7) + 1 AS DiaSemana,
           COUNT(*) AS Total
    FROM dbo.Agendamento a
    WHERE a.data_agendamento BETWEEN @DataInicio AND @DataFim
      AND a.status <> N'Cancelado'
    GROUP BY (DATEDIFF(DAY, '19000101', a.data_agendamento) % 7) + 1
    ORDER BY DiaSemana;
END
GO

PRINT N'Stored procedures do AgendaBella criadas com sucesso.';
GO
