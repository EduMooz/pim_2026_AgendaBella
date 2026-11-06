using System.ComponentModel.DataAnnotations;

namespace AgendaBella.Modelos;

public class PreferenciaCliente
{
    public int IdPreferencia { get; set; }
    public int IdCliente { get; set; }

    [StringLength(50)]  public string? FormatoUnha { get; set; }
    [StringLength(50)]  public string? CorFavorita { get; set; }
    [StringLength(100)] public string? Estilo { get; set; }
    [StringLength(500)] public string? Observacao { get; set; }

    public Cliente? Cliente { get; set; }
}
