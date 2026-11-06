using AgendaBella.Modelos;
using AgendaBella.Repositorio.Interfaces;

namespace AgendaBella.Negocio;

public class ProfissionalNegocio
{
    private readonly IProfissionalRepositorio _profissionais;
    public ProfissionalNegocio(IProfissionalRepositorio profissionais) => _profissionais = profissionais;

    // O AgendaBella atende uma profissional; o código já aceita mais de uma no banco.
    public Profissional ObterPadrao() =>
        _profissionais.ObterPadrao()
        ?? throw new RegraDeNegocioException("Nenhuma profissional ativa cadastrada.");
}
