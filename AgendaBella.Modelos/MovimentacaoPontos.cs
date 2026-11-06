namespace AgendaBella.Modelos;

public class MovimentacaoPontos
{
    public int IdMovimentacao { get; set; }
    public int IdCliente { get; set; }
    public int? IdAgendamento { get; set; }
    public int Quantidade { get; set; }
    public string Tipo { get; set; } = TipoPontos.Credito;
    public string? Descricao { get; set; }
    public DateTime DataMovimentacao { get; set; }

    // Crédito soma, débito subtrai.
    public int ValorComSinal => Tipo == TipoPontos.Credito ? Quantidade : -Quantidade;
}
