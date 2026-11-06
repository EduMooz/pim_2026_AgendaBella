using AgendaBella.Modelos;
using AgendaBella.Modelos.Dtos;
using AgendaBella.Repositorio.Interfaces;

namespace AgendaBella.Negocio;

// Dados da cliente e preferências de unhas.
public class ClienteNegocio
{
    private readonly IClienteRepositorio _clientes;
    private readonly IPreferenciaRepositorio _preferencias;

    public ClienteNegocio(IClienteRepositorio clientes, IPreferenciaRepositorio preferencias)
    {
        _clientes = clientes;
        _preferencias = preferencias;
    }

    public List<Cliente> Listar() => _clientes.Listar();

    public Cliente? ObterPorId(int id) => _clientes.ObterPorId(id);

    public PreferenciaCliente? ObterPreferencia(int idCliente) => _preferencias.ObterDoCliente(idCliente);

    // Cria a preferência na primeira vez; nas próximas, atualiza (relacionamento 1:1).
    public void SalvarPreferencia(int idCliente, PreferenciaDto dto)
    {
        if (_clientes.ObterPorId(idCliente) is null)
            throw new RegraDeNegocioException("Cliente não encontrada.");

        var existente = _preferencias.ObterDoCliente(idCliente);
        if (existente is null)
        {
            _preferencias.Adicionar(new PreferenciaCliente
            {
                IdCliente = idCliente,
                FormatoUnha = Limpar(dto.FormatoUnha),
                CorFavorita = Limpar(dto.CorFavorita),
                Estilo = Limpar(dto.Estilo),
                Observacao = Limpar(dto.Observacao)
            });
            return;
        }

        existente.FormatoUnha = Limpar(dto.FormatoUnha);
        existente.CorFavorita = Limpar(dto.CorFavorita);
        existente.Estilo = Limpar(dto.Estilo);
        existente.Observacao = Limpar(dto.Observacao);
        _preferencias.Salvar();
    }

    private static string? Limpar(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}
