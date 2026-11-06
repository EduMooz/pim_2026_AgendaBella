using AgendaBella.Modelos;
using AgendaBella.Repositorio.Contexto;
using AgendaBella.Repositorio.Interfaces;

namespace AgendaBella.Repositorio;

public class PreferenciaRepositorio : IPreferenciaRepositorio
{
    private readonly AppDbContext _contexto;
    public PreferenciaRepositorio(AppDbContext contexto) => _contexto = contexto;

    public PreferenciaCliente? ObterDoCliente(int idCliente) =>
        _contexto.PreferenciasCliente.FirstOrDefault(p => p.IdCliente == idCliente);

    public void Adicionar(PreferenciaCliente preferencia)
    {
        _contexto.PreferenciasCliente.Add(preferencia);
        _contexto.SaveChanges();
    }

    public void Salvar() => _contexto.SaveChanges();
}
