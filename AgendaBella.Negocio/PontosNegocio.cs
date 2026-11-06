using AgendaBella.Modelos;
using AgendaBella.Modelos.Dtos;
using AgendaBella.Repositorio.Interfaces;

namespace AgendaBella.Negocio;

// Programa de fidelidade. Os CRÉDITOS são gerados pela trigger TR_Agendamento_Pontos
// quando um atendimento é concluído; aqui ficam a consulta e o resgate (débito).
public class PontosNegocio
{
    private readonly IPontosRepositorio _pontos;
    private readonly IClienteRepositorio _clientes;

    public PontosNegocio(IPontosRepositorio pontos, IClienteRepositorio clientes)
    {
        _pontos = pontos;
        _clientes = clientes;
    }

    public SaldoPontosDto Obter(int idCliente) => new()
    {
        Saldo = _pontos.Saldo(idCliente),
        Extrato = _pontos.Extrato(idCliente).Select(MovimentacaoPontosDto.De).ToList()
    };

    public void Resgatar(ResgatarPontosDto dto)
    {
        if (_clientes.ObterPorId(dto.IdCliente) is null)
            throw new RegraDeNegocioException("Cliente não encontrada.");
        if (dto.Pontos <= 0)
            throw new RegraDeNegocioException("Informe uma quantidade de pontos maior que zero.");

        var saldo = _pontos.Saldo(dto.IdCliente);
        if (dto.Pontos > saldo)
            throw new RegraDeNegocioException($"Saldo insuficiente: a cliente tem {saldo} ponto(s).");

        _pontos.Adicionar(new MovimentacaoPontos
        {
            IdCliente = dto.IdCliente,
            Quantidade = dto.Pontos,
            Tipo = TipoPontos.Debito,
            Descricao = string.IsNullOrWhiteSpace(dto.Descricao) ? "Resgate de pontos" : dto.Descricao.Trim()
        });
    }
}
