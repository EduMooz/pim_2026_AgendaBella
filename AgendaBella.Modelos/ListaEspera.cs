namespace AgendaBella.Modelos;

public class ListaEspera
{
    public int IdListaEspera { get; set; }
    public int IdCliente { get; set; }
    public int? IdProfissional { get; set; }
    public int IdServico { get; set; }
    public DateTime DataDesejada { get; set; }
    public TimeSpan? HorarioDesejado { get; set; }
    public string Status { get; set; } = StatusListaEspera.Aguardando;
    public DateTime DataSolicitacao { get; set; }

    public Cliente? Cliente { get; set; }
    public Servico? Servico { get; set; }
}
