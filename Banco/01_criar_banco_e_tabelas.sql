/* =====================================================================
   AgendaBella  |  PIM IV  |  Modelo físico (SQL Server / T-SQL)
   Arquivo 01 - Criação do banco, tabelas, chaves, restrições e índices

   COMO EXECUTAR
     1) Abra este arquivo no SSMS (SQL Server Management Studio).
     2) Clique em "Executar" (F5). O script cria o banco AgendaBella
        e todas as tabelas. Pode ser executado de novo sem erro, porque
        cada tabela só é criada se ainda não existir.
     3) Depois execute, nesta ordem: 02, 03, 04 (e 05, opcional).

   POR QUE T-SQL E NÃO MySQL?
     O Manual do PIM IV exige banco SQL Server. Por isso o DDL em MySQL
     foi convertido: AUTO_INCREMENT virou IDENTITY, VARCHAR virou NVARCHAR,
     DATETIME DEFAULT CURRENT_TIMESTAMP virou DATETIME2 DEFAULT SYSDATETIME().
   ===================================================================== */

IF DB_ID(N'AgendaBella') IS NULL
    CREATE DATABASE AgendaBella;
GO

USE AgendaBella;
GO

-- Necessários para criar índices filtrados (usados mais abaixo).
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------------------------------------------------------------------
   1) CLIENTE
   A coluna senha guarda o HASH da senha (PBKDF2), nunca a senha pura.
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Cliente', N'U') IS NULL
CREATE TABLE dbo.Cliente (
    id_cliente     INT           IDENTITY(1,1) NOT NULL,
    nome           NVARCHAR(100) NOT NULL,
    email          NVARCHAR(150) NOT NULL,
    senha          NVARCHAR(255) NOT NULL,
    telefone       NVARCHAR(20)  NULL,
    ativo          BIT           NOT NULL CONSTRAINT DF_Cliente_ativo DEFAULT 1,
    data_cadastro  DATETIME2(0)  NOT NULL CONSTRAINT DF_Cliente_data_cadastro DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Cliente PRIMARY KEY (id_cliente),
    CONSTRAINT UQ_Cliente_email UNIQUE (email)
);
GO

/* ---------------------------------------------------------------------
   2) PROFISSIONAL (a manicure; também é a administradora do sistema)
   Diferença do DDL original: email e senha são NOT NULL, porque a
   profissional entra no painel administrativo com e-mail e senha.
   No SQL Server uma coluna UNIQUE aceita apenas UM valor NULL; no MySQL
   aceitaria vários. Tornar o e-mail obrigatório evita esse problema.
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Profissional', N'U') IS NULL
CREATE TABLE dbo.Profissional (
    id_profissional INT           IDENTITY(1,1) NOT NULL,
    nome            NVARCHAR(100) NOT NULL,
    email           NVARCHAR(150) NOT NULL,
    senha           NVARCHAR(255) NOT NULL,
    telefone        NVARCHAR(20)  NULL,
    ativo           BIT           NOT NULL CONSTRAINT DF_Profissional_ativo DEFAULT 1,
    CONSTRAINT PK_Profissional PRIMARY KEY (id_profissional),
    CONSTRAINT UQ_Profissional_email UNIQUE (email)
);
GO

/* ---------------------------------------------------------------------
   3) SERVICO
   "ativo" permite desativar um serviço sem apagar o histórico dos
   agendamentos que já o utilizaram (a chave estrangeira impediria o DELETE).
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Servico', N'U') IS NULL
CREATE TABLE dbo.Servico (
    id_servico INT           IDENTITY(1,1) NOT NULL,
    nome       NVARCHAR(100) NOT NULL,
    descricao  NVARCHAR(255) NULL,
    preco      DECIMAL(10,2) NOT NULL,
    duracao    INT           NOT NULL,          -- em minutos
    ativo      BIT           NOT NULL CONSTRAINT DF_Servico_ativo DEFAULT 1,
    CONSTRAINT PK_Servico PRIMARY KEY (id_servico),
    CONSTRAINT CK_Servico_preco   CHECK (preco >= 0),
    CONSTRAINT CK_Servico_duracao CHECK (duracao > 0)
);
GO

/* ---------------------------------------------------------------------
   4) HORARIO_AGENDAMENTO  (grade de horários de início oferecidos)
   Ex.: 09:00, 09:30, 10:00 ... A procedure sp_Horarios_Disponiveis
   cruza esta grade com o expediente, os bloqueios e os agendamentos.
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Horario_Agendamento', N'U') IS NULL
CREATE TABLE dbo.Horario_Agendamento (
    id_horario INT          IDENTITY(1,1) NOT NULL,
    horario    TIME(0)      NOT NULL,
    CONSTRAINT PK_Horario_Agendamento PRIMARY KEY (id_horario),
    CONSTRAINT UQ_Horario_Agendamento_horario UNIQUE (horario)
);
GO

/* ---------------------------------------------------------------------
   5) HORARIO_TRABALHO  (expediente semanal da profissional)
   dia_semana: 1 = segunda ... 7 = domingo (mesmo padrão do script antigo).
   Um dia pode ter mais de uma linha (ex.: manhã e tarde, com almoço).
   O DDL original usava VARCHAR(20); INT com CHECK evita "segunda",
   "Segunda-feira", "seg" misturados e facilita comparar com DayOfWeek.
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Horario_Trabalho', N'U') IS NULL
CREATE TABLE dbo.Horario_Trabalho (
    id_horario_trabalho INT     IDENTITY(1,1) NOT NULL,
    id_profissional     INT     NOT NULL,
    dia_semana          INT     NOT NULL,
    hora_inicio         TIME(0) NOT NULL,
    hora_fim            TIME(0) NOT NULL,
    CONSTRAINT PK_Horario_Trabalho PRIMARY KEY (id_horario_trabalho),
    CONSTRAINT FK_Horario_Trabalho_Profissional FOREIGN KEY (id_profissional)
        REFERENCES dbo.Profissional (id_profissional),
    CONSTRAINT CK_Horario_Trabalho_dia    CHECK (dia_semana BETWEEN 1 AND 7),
    CONSTRAINT CK_Horario_Trabalho_ordem  CHECK (hora_inicio < hora_fim)
);
GO

/* ---------------------------------------------------------------------
   6) BLOQUEIO_HORARIO  (folga, feriado, curso, consulta médica...)
   hora_inicio e hora_fim NULOS = bloqueio do dia inteiro.
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Bloqueio_Horario', N'U') IS NULL
CREATE TABLE dbo.Bloqueio_Horario (
    id_bloqueio     INT           IDENTITY(1,1) NOT NULL,
    id_profissional INT           NOT NULL,
    data_bloqueio   DATE          NOT NULL,
    hora_inicio     TIME(0)       NULL,
    hora_fim        TIME(0)       NULL,
    motivo          NVARCHAR(255) NULL,
    CONSTRAINT PK_Bloqueio_Horario PRIMARY KEY (id_bloqueio),
    CONSTRAINT FK_Bloqueio_Horario_Profissional FOREIGN KEY (id_profissional)
        REFERENCES dbo.Profissional (id_profissional),
    CONSTRAINT CK_Bloqueio_Horario_ordem CHECK
        (hora_inicio IS NULL OR hora_fim IS NULL OR hora_inicio < hora_fim)
);
GO

/* ---------------------------------------------------------------------
   7) AGENDAMENTO  (tabela central do sistema)
   - status só aceita 4 valores (CHECK).
   - imagem_inspiracao guarda o CAMINHO do arquivo enviado (não a imagem).
   - Existem TRIGGERS nesta tabela (arquivo 02): histórico, pontos e
     lista de espera. Por isso o EF Core precisa de HasTrigger no mapeamento.
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Agendamento', N'U') IS NULL
CREATE TABLE dbo.Agendamento (
    id_agendamento    INT           IDENTITY(1,1) NOT NULL,
    id_cliente        INT           NOT NULL,
    id_profissional   INT           NOT NULL,
    id_servico        INT           NOT NULL,
    data_agendamento  DATE          NOT NULL,
    horario           TIME(0)       NOT NULL,
    status            NVARCHAR(30)  NOT NULL CONSTRAINT DF_Agendamento_status DEFAULT N'Pendente',
    imagem_inspiracao NVARCHAR(500) NULL,
    observacao        NVARCHAR(500) NULL,
    data_criacao      DATETIME2(0)  NOT NULL CONSTRAINT DF_Agendamento_data_criacao DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Agendamento PRIMARY KEY (id_agendamento),
    CONSTRAINT FK_Agendamento_Cliente      FOREIGN KEY (id_cliente)      REFERENCES dbo.Cliente (id_cliente),
    CONSTRAINT FK_Agendamento_Profissional FOREIGN KEY (id_profissional) REFERENCES dbo.Profissional (id_profissional),
    CONSTRAINT FK_Agendamento_Servico      FOREIGN KEY (id_servico)      REFERENCES dbo.Servico (id_servico),
    CONSTRAINT CK_Agendamento_status CHECK
        (status IN (N'Pendente', N'Confirmado', N'Concluido', N'Cancelado'))
);
GO

/* Dois agendamentos ativos não podem ocupar o mesmo profissional/dia/horário.
   O índice é FILTRADO: agendamentos cancelados ficam de fora, então um
   horário cancelado pode ser reservado de novo. (O script antigo usava
   UNIQUE simples, que impedia reaproveitar um horário cancelado.)        */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Agendamento_Profissional_DataHora')
    CREATE UNIQUE INDEX UX_Agendamento_Profissional_DataHora
        ON dbo.Agendamento (id_profissional, data_agendamento, horario)
        WHERE status <> N'Cancelado';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Agendamento_Cliente_Data')
    CREATE INDEX IX_Agendamento_Cliente_Data ON dbo.Agendamento (id_cliente, data_agendamento);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Agendamento_Data')
    CREATE INDEX IX_Agendamento_Data ON dbo.Agendamento (data_agendamento);
GO

/* ---------------------------------------------------------------------
   8) LISTA_ESPERA  (cliente quer um dia que está lotado)
   Fluxo do status:  Aguardando -> Notificado (trigger) -> Atendido
                                                      \-> Cancelado
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Lista_Espera', N'U') IS NULL
CREATE TABLE dbo.Lista_Espera (
    id_lista_espera  INT          IDENTITY(1,1) NOT NULL,
    id_cliente       INT          NOT NULL,
    id_profissional  INT          NULL,        -- NULL = qualquer profissional
    id_servico       INT          NOT NULL,
    data_desejada    DATE         NOT NULL,
    horario_desejado TIME(0)      NULL,        -- NULL = qualquer horário do dia
    status           NVARCHAR(30) NOT NULL CONSTRAINT DF_Lista_Espera_status DEFAULT N'Aguardando',
    data_solicitacao DATETIME2(0) NOT NULL CONSTRAINT DF_Lista_Espera_data DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Lista_Espera PRIMARY KEY (id_lista_espera),
    CONSTRAINT FK_Lista_Espera_Cliente      FOREIGN KEY (id_cliente)      REFERENCES dbo.Cliente (id_cliente),
    CONSTRAINT FK_Lista_Espera_Profissional FOREIGN KEY (id_profissional) REFERENCES dbo.Profissional (id_profissional),
    CONSTRAINT FK_Lista_Espera_Servico      FOREIGN KEY (id_servico)      REFERENCES dbo.Servico (id_servico),
    CONSTRAINT CK_Lista_Espera_status CHECK
        (status IN (N'Aguardando', N'Notificado', N'Atendido', N'Cancelado'))
);
GO

-- Impede a mesma cliente de entrar duas vezes na fila do mesmo serviço/dia.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Lista_Espera_Ativa')
    CREATE UNIQUE INDEX UX_Lista_Espera_Ativa
        ON dbo.Lista_Espera (id_cliente, id_servico, data_desejada)
        WHERE status IN (N'Aguardando', N'Notificado');
GO

/* ---------------------------------------------------------------------
   9) PREFERENCIA_CLIENTE  (relacionamento 1 para 1 com Cliente)
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Preferencia_Cliente', N'U') IS NULL
CREATE TABLE dbo.Preferencia_Cliente (
    id_preferencia INT           IDENTITY(1,1) NOT NULL,
    id_cliente     INT           NOT NULL,
    formato_unha   NVARCHAR(50)  NULL,
    cor_favorita   NVARCHAR(50)  NULL,
    estilo         NVARCHAR(100) NULL,
    observacao     NVARCHAR(500) NULL,
    CONSTRAINT PK_Preferencia_Cliente PRIMARY KEY (id_preferencia),
    CONSTRAINT UQ_Preferencia_Cliente_cliente UNIQUE (id_cliente),
    CONSTRAINT FK_Preferencia_Cliente_Cliente FOREIGN KEY (id_cliente) REFERENCES dbo.Cliente (id_cliente)
);
GO

/* ---------------------------------------------------------------------
   10) MOVIMENTACAO_PONTOS  (programa de fidelidade)
   quantidade é sempre positiva; o campo tipo diz se soma ou subtrai.
   Saldo = soma dos Créditos - soma dos Débitos.
   id_agendamento liga o crédito ao atendimento que o gerou.
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Movimentacao_Pontos', N'U') IS NULL
CREATE TABLE dbo.Movimentacao_Pontos (
    id_movimentacao   INT           IDENTITY(1,1) NOT NULL,
    id_cliente        INT           NOT NULL,
    id_agendamento    INT           NULL,
    quantidade        INT           NOT NULL,
    tipo              NVARCHAR(30)  NOT NULL,
    descricao         NVARCHAR(255) NULL,
    data_movimentacao DATETIME2(0)  NOT NULL CONSTRAINT DF_Movimentacao_Pontos_data DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Movimentacao_Pontos PRIMARY KEY (id_movimentacao),
    CONSTRAINT FK_Movimentacao_Pontos_Cliente     FOREIGN KEY (id_cliente)     REFERENCES dbo.Cliente (id_cliente),
    CONSTRAINT FK_Movimentacao_Pontos_Agendamento FOREIGN KEY (id_agendamento) REFERENCES dbo.Agendamento (id_agendamento),
    CONSTRAINT CK_Movimentacao_Pontos_qtd  CHECK (quantidade > 0),
    CONSTRAINT CK_Movimentacao_Pontos_tipo CHECK (tipo IN (N'Credito', N'Debito'))
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Movimentacao_Pontos_Cliente')
    CREATE INDEX IX_Movimentacao_Pontos_Cliente ON dbo.Movimentacao_Pontos (id_cliente, data_movimentacao);
GO

/* ---------------------------------------------------------------------
   11) HISTORICO_AGENDAMENTO  (tabela nova: o "histórico" pedido)
   Preenchida AUTOMATICAMENTE pela trigger TR_Agendamento_Historico.
   O sistema nunca grava nela: apenas consulta.
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Historico_Agendamento', N'U') IS NULL
CREATE TABLE dbo.Historico_Agendamento (
    id_historico     INT          IDENTITY(1,1) NOT NULL,
    id_agendamento   INT          NOT NULL,
    tipo_evento      NVARCHAR(30) NOT NULL,
    status_anterior  NVARCHAR(30) NULL,
    status_novo      NVARCHAR(30) NOT NULL,
    data_anterior    DATE         NULL,
    horario_anterior TIME(0)      NULL,
    data_nova        DATE         NOT NULL,
    horario_novo     TIME(0)      NOT NULL,
    data_evento      DATETIME2(0) NOT NULL CONSTRAINT DF_Historico_Agendamento_data DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Historico_Agendamento PRIMARY KEY (id_historico),
    CONSTRAINT FK_Historico_Agendamento_Agendamento FOREIGN KEY (id_agendamento)
        REFERENCES dbo.Agendamento (id_agendamento),
    CONSTRAINT CK_Historico_Agendamento_tipo CHECK
        (tipo_evento IN (N'Criacao', N'MudancaStatus', N'Reagendamento'))
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Historico_Agendamento_Agendamento')
    CREATE INDEX IX_Historico_Agendamento_Agendamento ON dbo.Historico_Agendamento (id_agendamento, data_evento);
GO

PRINT N'Tabelas do AgendaBella criadas com sucesso.';
GO
