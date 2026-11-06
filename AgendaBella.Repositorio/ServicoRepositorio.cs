using AgendaBella.Modelos;
using AgendaBella.Repositorio.Contexto;
using AgendaBella.Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendaBella.Repositorio;

public class ServicoRepositorio : IServicoRepositorio
{
    private readonly AppDbContext _contexto;
    public ServicoRepositorio(AppDbContext contexto) => _contexto = contexto;

    public List<Servico> Listar(bool somenteAtivos)
    {
        var consulta = _contexto.Servicos.AsNoTracking().AsQueryable();
        if (somenteAtivos) consulta = consulta.Where(s => s.Ativo);
        return consulta.OrderBy(s => s.Nome).ToList();
    }

    public Servico? ObterPorId(int id) =>
        _contexto.Servicos.FirstOrDefault(s => s.IdServico == id);

    public bool PossuiAgendamentos(int idServico) =>
        _contexto.Agendamentos.Any(a => a.IdServico == idServico);

    public void Adicionar(Servico servico)
    {
        _contexto.Servicos.Add(servico);
        _contexto.SaveChanges();
    }

    public void Remover(Servico servico)
    {
        _contexto.Servicos.Remove(servico);
        _contexto.SaveChanges();
    }

    public void Salvar() => _contexto.SaveChanges();
}
