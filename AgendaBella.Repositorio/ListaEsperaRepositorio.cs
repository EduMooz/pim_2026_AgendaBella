using AgendaBella.Modelos;
using AgendaBella.Repositorio.Contexto;
using AgendaBella.Repositorio.Excecoes;
using AgendaBella.Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendaBella.Repositorio;

public class ListaEsperaRepositorio : IListaEsperaRepositorio
{
    private readonly AppDbContext _contexto;
    public ListaEsperaRepositorio(AppDbContext contexto) => _contexto = contexto;

    public List<ListaEspera> ListarDoCliente(int idCliente) =>
        _contexto.ListasEspera.AsNoTracking()
            .Include(l => l.Servico)
            .Include(l => l.Cliente)
            .Where(l => l.IdCliente == idCliente)
            .OrderByDescending(l => l.DataSolicitacao)
            .ToList();

    public List<ListaEspera> ListarAtivas() =>
        _contexto.ListasEspera.AsNoTracking()
            .Include(l => l.Servico)
            .Include(l => l.Cliente)
            .Where(l => l.Status == StatusListaEspera.Aguardando || l.Status == StatusListaEspera.Notificado)
            .OrderBy(l => l.DataDesejada).ThenBy(l => l.DataSolicitacao)
            .ToList();

    public ListaEspera? ObterPorId(int id) =>
        _contexto.ListasEspera.FirstOrDefault(l => l.IdListaEspera == id);

    public bool JaExisteAtiva(int idCliente, int idServico, DateTime data)
    {
        var dia = data.Date;
        return _contexto.ListasEspera.Any(l =>
            l.IdCliente == idCliente && l.IdServico == idServico && l.DataDesejada == dia &&
            (l.Status == StatusListaEspera.Aguardando || l.Status == StatusListaEspera.Notificado));
    }

    public int ContarAguardando() =>
        _contexto.ListasEspera.Count(l => l.Status == StatusListaEspera.Aguardando);

    public void MarcarAtendidas(int idCliente, DateTime data)
    {
        var dia = data.Date;
        var itens = _contexto.ListasEspera
            .Where(l => l.IdCliente == idCliente && l.DataDesejada == dia &&
                        l.Status == StatusListaEspera.Notificado)
            .ToList();
        if (itens.Count == 0) return;

        foreach (var item in itens) item.Status = StatusListaEspera.Atendido;
        _contexto.SaveChanges();
    }

    public void Adicionar(ListaEspera item)
    {
        _contexto.ListasEspera.Add(item);
        try
        {
            _contexto.SaveChanges();
        }
        catch (DbUpdateException ex) when (ErrosSql.EhViolacaoDeUnicidade(ex))
        {
            throw new ConflitoDeDadosException("Você já está na lista de espera deste serviço neste dia.", ex);
        }
    }

    public void Salvar() => _contexto.SaveChanges();
}
