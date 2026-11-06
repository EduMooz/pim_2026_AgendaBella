using AgendaBella.Modelos;
using AgendaBella.Repositorio.Contexto;
using AgendaBella.Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendaBella.Repositorio;

public class PontosRepositorio : IPontosRepositorio
{
    private readonly AppDbContext _contexto;
    public PontosRepositorio(AppDbContext contexto) => _contexto = contexto;

    public List<MovimentacaoPontos> Extrato(int idCliente) =>
        _contexto.MovimentacoesPontos.AsNoTracking()
            .Where(m => m.IdCliente == idCliente)
            .OrderByDescending(m => m.DataMovimentacao).ThenByDescending(m => m.IdMovimentacao)
            .ToList();

    // Saldo = créditos - débitos, calculado pelo próprio banco (SUM).
    public int Saldo(int idCliente) =>
        _contexto.MovimentacoesPontos
            .Where(m => m.IdCliente == idCliente)
            .Sum(m => m.Tipo == TipoPontos.Credito ? m.Quantidade : -m.Quantidade);

    public void Adicionar(MovimentacaoPontos movimentacao)
    {
        _contexto.MovimentacoesPontos.Add(movimentacao);
        _contexto.SaveChanges();
    }
}
