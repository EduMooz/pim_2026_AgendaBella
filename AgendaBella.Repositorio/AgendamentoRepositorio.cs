using System.Data;
using AgendaBella.Modelos;
using AgendaBella.Modelos.Relatorios;
using AgendaBella.Repositorio.Contexto;
using AgendaBella.Repositorio.Excecoes;
using AgendaBella.Repositorio.Interfaces;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace AgendaBella.Repositorio;

// Combinação sugerida no manual:
//   - Entity Framework Core para cadastro/consulta (LINQ);
//   - Dapper para executar a stored procedure sp_Horarios_Disponiveis.
public class AgendamentoRepositorio : IAgendamentoRepositorio
{
    private readonly AppDbContext _contexto;
    public AgendamentoRepositorio(AppDbContext contexto) => _contexto = contexto;

    // ---------- Entity Framework Core ----------

    // Include = traz junto os dados relacionados (cliente, serviço) em um único SELECT com JOIN.
    public Agendamento? ObterPorId(int id) =>
        _contexto.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Servico)
            .FirstOrDefault(a => a.IdAgendamento == id);

    public List<Agendamento> ListarDoCliente(int idCliente) =>
        _contexto.Agendamentos.AsNoTracking()
            .Include(a => a.Cliente)
            .Include(a => a.Servico)
            .Where(a => a.IdCliente == idCliente)
            .OrderByDescending(a => a.DataAgendamento).ThenByDescending(a => a.Horario)
            .ToList();

    public List<Agendamento> ListarDoDia(DateTime data)
    {
        var dia = data.Date;
        return _contexto.Agendamentos.AsNoTracking()
            .Include(a => a.Cliente).ThenInclude(c => c!.Preferencia)
            .Include(a => a.Servico)
            .Where(a => a.DataAgendamento == dia)
            .OrderBy(a => a.Horario)
            .ToList();
    }

    public List<Agendamento> ListarProximos(int quantidade)
    {
        var hoje = DateTime.Today;
        return _contexto.Agendamentos.AsNoTracking()
            .Include(a => a.Cliente)
            .Include(a => a.Servico)
            .Where(a => a.DataAgendamento >= hoje &&
                        (a.Status == StatusAgendamento.Pendente || a.Status == StatusAgendamento.Confirmado))
            .OrderBy(a => a.DataAgendamento).ThenBy(a => a.Horario)
            .Take(quantidade)
            .ToList();
    }

    public int ContarDoDia(DateTime data)
    {
        var dia = data.Date;
        return _contexto.Agendamentos.Count(a => a.DataAgendamento == dia && a.Status != StatusAgendamento.Cancelado);
    }

    public int ContarPendentes() =>
        _contexto.Agendamentos.Count(a => a.Status == StatusAgendamento.Pendente);

    public bool ClienteTemAgendamentoNoDia(int idCliente, DateTime data, int? ignorarId = null)
    {
        var dia = data.Date;
        return _contexto.Agendamentos.Any(a =>
            a.IdCliente == idCliente &&
            a.DataAgendamento == dia &&
            a.Status != StatusAgendamento.Cancelado &&
            a.IdAgendamento != ignorarId);
    }

    public List<HistoricoAgendamento> ListarHistorico(int idAgendamento) =>
        _contexto.HistoricosAgendamento.AsNoTracking()
            .Where(h => h.IdAgendamento == idAgendamento)
            .OrderBy(h => h.DataEvento).ThenBy(h => h.IdHistorico)
            .ToList();

    public void Adicionar(Agendamento agendamento)
    {
        _contexto.Agendamentos.Add(agendamento);
        GravarTraduzindoConflito();
    }

    public void Salvar() => GravarTraduzindoConflito();

    // Se o banco recusar por índice único (horário já ocupado), vira ConflitoDeDadosException.
    private void GravarTraduzindoConflito()
    {
        try
        {
            _contexto.SaveChanges();
        }
        catch (DbUpdateException ex) when (ErrosSql.EhViolacaoDeUnicidade(ex))
        {
            throw new ConflitoDeDadosException("Este horário acabou de ser ocupado por outra cliente.", ex);
        }
    }

    // ---------- Dapper + stored procedure ----------

    public List<HorarioDisponivel> ListarHorariosDisponiveis(
        int idProfissional, DateTime data, int idServico, int? ignorarAgendamento = null)
    {
        // Reaproveita a MESMA conexão (e a mesma connection string) do EF Core.
        var conexao = _contexto.Database.GetDbConnection();

        // Parâmetros vão separados do comando: é isso que impede SQL Injection.
        var horarios = conexao.Query<HorarioDisponivel>(
            "sp_Horarios_Disponiveis",
            new
            {
                IdProfissional = idProfissional,
                Data = data.Date,
                IdServico = idServico,
                IgnorarAgendamento = ignorarAgendamento
            },
            commandType: CommandType.StoredProcedure);

        return horarios.ToList();
    }
}
