using AgendaBella.Modelos;

namespace AgendaBella.Repositorio.Interfaces;

public interface IProfissionalRepositorio
{
    Profissional? ObterPorEmail(string email);
    Profissional? ObterPorId(int id);
    Profissional? ObterPadrao();     // primeira profissional ativa
}
