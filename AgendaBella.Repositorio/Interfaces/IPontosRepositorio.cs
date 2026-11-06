using AgendaBella.Modelos;

namespace AgendaBella.Repositorio.Interfaces;

public interface IPontosRepositorio
{
    List<MovimentacaoPontos> Extrato(int idCliente);
    int Saldo(int idCliente);
    void Adicionar(MovimentacaoPontos movimentacao);
}
