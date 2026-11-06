using AgendaBella.Modelos;
using AgendaBella.Repositorio.Contexto;
using AgendaBella.Repositorio.Interfaces;

namespace AgendaBella.Repositorio;

public class ProfissionalRepositorio : IProfissionalRepositorio
{
    private readonly AppDbContext _contexto;
    public ProfissionalRepositorio(AppDbContext contexto) => _contexto = contexto;

    public Profissional? ObterPorEmail(string email) =>
        _contexto.Profissionais.FirstOrDefault(p => p.Email == email && p.Ativo);

    public Profissional? ObterPorId(int id) =>
        _contexto.Profissionais.FirstOrDefault(p => p.IdProfissional == id);

    public Profissional? ObterPadrao() =>
        _contexto.Profissionais.Where(p => p.Ativo).OrderBy(p => p.IdProfissional).FirstOrDefault();
}
