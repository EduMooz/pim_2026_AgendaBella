using AgendaBella.Modelos;
using Microsoft.EntityFrameworkCore;

namespace AgendaBella.Repositorio.Contexto;

// O DbContext é a "porta" do Entity Framework Core para o banco.
// Cada DbSet representa uma tabela. O banco é criado pelos scripts da pasta /Banco;
// aqui o EF só aprende COMO as classes C# se ligam às tabelas e colunas (mapeamento).
public class AppDbContext : DbContext
{
    // As "options" trazem o SQL Server e a connection string (montadas no Program.cs).
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Profissional> Profissionais => Set<Profissional>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<HorarioAgendamento> HorariosAgendamento => Set<HorarioAgendamento>();
    public DbSet<HorarioTrabalho> HorariosTrabalho => Set<HorarioTrabalho>();
    public DbSet<BloqueioHorario> BloqueiosHorario => Set<BloqueioHorario>();
    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();
    public DbSet<ListaEspera> ListasEspera => Set<ListaEspera>();
    public DbSet<PreferenciaCliente> PreferenciasCliente => Set<PreferenciaCliente>();
    public DbSet<MovimentacaoPontos> MovimentacoesPontos => Set<MovimentacaoPontos>();
    public DbSet<HistoricoAgendamento> HistoricosAgendamento => Set<HistoricoAgendamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---------------- Cliente ----------------
        modelBuilder.Entity<Cliente>(e =>
        {
            e.ToTable("Cliente");
            e.HasKey(c => c.IdCliente);
            e.Property(c => c.IdCliente).HasColumnName("id_cliente");
            e.Property(c => c.Nome).HasColumnName("nome").HasMaxLength(100).IsRequired();
            e.Property(c => c.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
            e.Property(c => c.Senha).HasColumnName("senha").HasMaxLength(255).IsRequired();
            e.Property(c => c.Telefone).HasColumnName("telefone").HasMaxLength(20);
            e.Property(c => c.Ativo).HasColumnName("ativo");
            e.Property(c => c.DataCadastro).HasColumnName("data_cadastro")
                .HasColumnType("datetime2(0)").HasDefaultValueSql("SYSDATETIME()");
            e.HasIndex(c => c.Email).IsUnique().HasDatabaseName("UQ_Cliente_email");

            // 1 cliente tem no máximo 1 preferência (relacionamento 1:1).
            e.HasOne(c => c.Preferencia).WithOne(p => p.Cliente)
                .HasForeignKey<PreferenciaCliente>(p => p.IdCliente).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(c => c.Agendamentos).WithOne(a => a.Cliente)
                .HasForeignKey(a => a.IdCliente).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(c => c.Pontos).WithOne()
                .HasForeignKey(m => m.IdCliente).OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------- Profissional ----------------
        modelBuilder.Entity<Profissional>(e =>
        {
            e.ToTable("Profissional");
            e.HasKey(p => p.IdProfissional);
            e.Property(p => p.IdProfissional).HasColumnName("id_profissional");
            e.Property(p => p.Nome).HasColumnName("nome").HasMaxLength(100).IsRequired();
            e.Property(p => p.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
            e.Property(p => p.Senha).HasColumnName("senha").HasMaxLength(255).IsRequired();
            e.Property(p => p.Telefone).HasColumnName("telefone").HasMaxLength(20);
            e.Property(p => p.Ativo).HasColumnName("ativo");
            e.HasIndex(p => p.Email).IsUnique().HasDatabaseName("UQ_Profissional_email");
            e.HasMany(p => p.HorariosTrabalho).WithOne(h => h.Profissional)
                .HasForeignKey(h => h.IdProfissional).OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------- Servico ----------------
        modelBuilder.Entity<Servico>(e =>
        {
            e.ToTable("Servico", t =>
            {
                t.HasCheckConstraint("CK_Servico_preco", "[preco] >= 0");
                t.HasCheckConstraint("CK_Servico_duracao", "[duracao] > 0");
            });
            e.HasKey(s => s.IdServico);
            e.Property(s => s.IdServico).HasColumnName("id_servico");
            e.Property(s => s.Nome).HasColumnName("nome").HasMaxLength(100).IsRequired();
            e.Property(s => s.Descricao).HasColumnName("descricao").HasMaxLength(255);
            e.Property(s => s.Preco).HasColumnName("preco").HasColumnType("decimal(10,2)");
            e.Property(s => s.Duracao).HasColumnName("duracao");
            e.Property(s => s.Ativo).HasColumnName("ativo");
        });

        // ---------------- Horario_Agendamento ----------------
        modelBuilder.Entity<HorarioAgendamento>(e =>
        {
            e.ToTable("Horario_Agendamento");
            e.HasKey(h => h.IdHorario);
            e.Property(h => h.IdHorario).HasColumnName("id_horario");
            e.Property(h => h.Horario).HasColumnName("horario").HasColumnType("time(0)");
            e.HasIndex(h => h.Horario).IsUnique().HasDatabaseName("UQ_Horario_Agendamento_horario");
        });

        // ---------------- Horario_Trabalho ----------------
        modelBuilder.Entity<HorarioTrabalho>(e =>
        {
            e.ToTable("Horario_Trabalho", t =>
            {
                t.HasCheckConstraint("CK_Horario_Trabalho_dia", "[dia_semana] BETWEEN 1 AND 7");
                t.HasCheckConstraint("CK_Horario_Trabalho_ordem", "[hora_inicio] < [hora_fim]");
            });
            e.HasKey(h => h.IdHorarioTrabalho);
            e.Property(h => h.IdHorarioTrabalho).HasColumnName("id_horario_trabalho");
            e.Property(h => h.IdProfissional).HasColumnName("id_profissional");
            e.Property(h => h.DiaSemana).HasColumnName("dia_semana");
            e.Property(h => h.HoraInicio).HasColumnName("hora_inicio").HasColumnType("time(0)");
            e.Property(h => h.HoraFim).HasColumnName("hora_fim").HasColumnType("time(0)");
        });

        // ---------------- Bloqueio_Horario ----------------
        modelBuilder.Entity<BloqueioHorario>(e =>
        {
            e.ToTable("Bloqueio_Horario");
            e.HasKey(b => b.IdBloqueio);
            e.Property(b => b.IdBloqueio).HasColumnName("id_bloqueio");
            e.Property(b => b.IdProfissional).HasColumnName("id_profissional");
            e.Property(b => b.DataBloqueio).HasColumnName("data_bloqueio").HasColumnType("date");
            e.Property(b => b.HoraInicio).HasColumnName("hora_inicio").HasColumnType("time(0)");
            e.Property(b => b.HoraFim).HasColumnName("hora_fim").HasColumnType("time(0)");
            e.Property(b => b.Motivo).HasColumnName("motivo").HasMaxLength(255);
            e.HasOne(b => b.Profissional).WithMany()
                .HasForeignKey(b => b.IdProfissional).OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------- Agendamento ----------------
        modelBuilder.Entity<Agendamento>(e =>
        {
            e.ToTable("Agendamento", t =>
            {
                // OBRIGATÓRIO (EF Core 7+) em tabela com trigger: uma chamada para CADA trigger.
                // Sem isto, todo INSERT/UPDATE em Agendamento falha, porque o EF usa OUTPUT,
                // que o SQL Server não aceita em tabela com trigger.
                t.HasTrigger("TR_Agendamento_Historico");
                t.HasTrigger("TR_Agendamento_Pontos");
                t.HasTrigger("TR_Agendamento_ListaEspera");

                t.HasCheckConstraint("CK_Agendamento_status",
                    "[status] IN (N'Pendente', N'Confirmado', N'Concluido', N'Cancelado')");
            });

            e.HasKey(a => a.IdAgendamento);
            e.Property(a => a.IdAgendamento).HasColumnName("id_agendamento");
            e.Property(a => a.IdCliente).HasColumnName("id_cliente");
            e.Property(a => a.IdProfissional).HasColumnName("id_profissional");
            e.Property(a => a.IdServico).HasColumnName("id_servico");
            e.Property(a => a.DataAgendamento).HasColumnName("data_agendamento").HasColumnType("date");
            e.Property(a => a.Horario).HasColumnName("horario").HasColumnType("time(0)");
            e.Property(a => a.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            e.Property(a => a.ImagemInspiracao).HasColumnName("imagem_inspiracao").HasMaxLength(500);
            e.Property(a => a.Observacao).HasColumnName("observacao").HasMaxLength(500);
            e.Property(a => a.DataCriacao).HasColumnName("data_criacao")
                .HasColumnType("datetime2(0)").HasDefaultValueSql("SYSDATETIME()");

            // Índice único FILTRADO: cancelados não bloqueiam o horário.
            e.HasIndex(a => new { a.IdProfissional, a.DataAgendamento, a.Horario })
                .IsUnique()
                .HasFilter("[status] <> N'Cancelado'")
                .HasDatabaseName("UX_Agendamento_Profissional_DataHora");

            e.HasOne(a => a.Profissional).WithMany()
                .HasForeignKey(a => a.IdProfissional).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Servico).WithMany()
                .HasForeignKey(a => a.IdServico).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(a => a.Historico).WithOne()
                .HasForeignKey(h => h.IdAgendamento).OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------- Lista_Espera ----------------
        modelBuilder.Entity<ListaEspera>(e =>
        {
            e.ToTable("Lista_Espera");
            e.HasKey(l => l.IdListaEspera);
            e.Property(l => l.IdListaEspera).HasColumnName("id_lista_espera");
            e.Property(l => l.IdCliente).HasColumnName("id_cliente");
            e.Property(l => l.IdProfissional).HasColumnName("id_profissional");
            e.Property(l => l.IdServico).HasColumnName("id_servico");
            e.Property(l => l.DataDesejada).HasColumnName("data_desejada").HasColumnType("date");
            e.Property(l => l.HorarioDesejado).HasColumnName("horario_desejado").HasColumnType("time(0)");
            e.Property(l => l.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            e.Property(l => l.DataSolicitacao).HasColumnName("data_solicitacao")
                .HasColumnType("datetime2(0)").HasDefaultValueSql("SYSDATETIME()");

            e.HasOne(l => l.Cliente).WithMany()
                .HasForeignKey(l => l.IdCliente).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(l => l.Servico).WithMany()
                .HasForeignKey(l => l.IdServico).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Profissional>().WithMany()
                .HasForeignKey(l => l.IdProfissional).OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------- Preferencia_Cliente ----------------
        modelBuilder.Entity<PreferenciaCliente>(e =>
        {
            e.ToTable("Preferencia_Cliente");
            e.HasKey(p => p.IdPreferencia);
            e.Property(p => p.IdPreferencia).HasColumnName("id_preferencia");
            e.Property(p => p.IdCliente).HasColumnName("id_cliente");
            e.Property(p => p.FormatoUnha).HasColumnName("formato_unha").HasMaxLength(50);
            e.Property(p => p.CorFavorita).HasColumnName("cor_favorita").HasMaxLength(50);
            e.Property(p => p.Estilo).HasColumnName("estilo").HasMaxLength(100);
            e.Property(p => p.Observacao).HasColumnName("observacao").HasMaxLength(500);
            e.HasIndex(p => p.IdCliente).IsUnique().HasDatabaseName("UQ_Preferencia_Cliente_cliente");
        });

        // ---------------- Movimentacao_Pontos ----------------
        modelBuilder.Entity<MovimentacaoPontos>(e =>
        {
            e.ToTable("Movimentacao_Pontos", t =>
            {
                t.HasCheckConstraint("CK_Movimentacao_Pontos_qtd", "[quantidade] > 0");
                t.HasCheckConstraint("CK_Movimentacao_Pontos_tipo", "[tipo] IN (N'Credito', N'Debito')");
            });
            e.HasKey(m => m.IdMovimentacao);
            e.Property(m => m.IdMovimentacao).HasColumnName("id_movimentacao");
            e.Property(m => m.IdCliente).HasColumnName("id_cliente");
            e.Property(m => m.IdAgendamento).HasColumnName("id_agendamento");
            e.Property(m => m.Quantidade).HasColumnName("quantidade");
            e.Property(m => m.Tipo).HasColumnName("tipo").HasMaxLength(30).IsRequired();
            e.Property(m => m.Descricao).HasColumnName("descricao").HasMaxLength(255);
            e.Property(m => m.DataMovimentacao).HasColumnName("data_movimentacao")
                .HasColumnType("datetime2(0)").HasDefaultValueSql("SYSDATETIME()");
            e.HasOne<Agendamento>().WithMany()
                .HasForeignKey(m => m.IdAgendamento).OnDelete(DeleteBehavior.Restrict);
        });

        // ---------------- Historico_Agendamento ----------------
        modelBuilder.Entity<HistoricoAgendamento>(e =>
        {
            e.ToTable("Historico_Agendamento");
            e.HasKey(h => h.IdHistorico);
            e.Property(h => h.IdHistorico).HasColumnName("id_historico");
            e.Property(h => h.IdAgendamento).HasColumnName("id_agendamento");
            e.Property(h => h.TipoEvento).HasColumnName("tipo_evento").HasMaxLength(30).IsRequired();
            e.Property(h => h.StatusAnterior).HasColumnName("status_anterior").HasMaxLength(30);
            e.Property(h => h.StatusNovo).HasColumnName("status_novo").HasMaxLength(30).IsRequired();
            e.Property(h => h.DataAnterior).HasColumnName("data_anterior").HasColumnType("date");
            e.Property(h => h.HorarioAnterior).HasColumnName("horario_anterior").HasColumnType("time(0)");
            e.Property(h => h.DataNova).HasColumnName("data_nova").HasColumnType("date");
            e.Property(h => h.HorarioNovo).HasColumnName("horario_novo").HasColumnType("time(0)");
            e.Property(h => h.DataEvento).HasColumnName("data_evento")
                .HasColumnType("datetime2(0)").HasDefaultValueSql("SYSDATETIME()");
        });
    }
}
