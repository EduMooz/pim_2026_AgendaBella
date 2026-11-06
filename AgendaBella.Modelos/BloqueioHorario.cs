using System.ComponentModel.DataAnnotations;

namespace AgendaBella.Modelos;

// Folga/feriado/compromisso. HoraInicio e HoraFim nulos = dia inteiro.
public class BloqueioHorario
{
    public int IdBloqueio { get; set; }
    public int IdProfissional { get; set; }
    public DateTime DataBloqueio { get; set; }
    public TimeSpan? HoraInicio { get; set; }
    public TimeSpan? HoraFim { get; set; }

    [StringLength(255)]
    public string? Motivo { get; set; }

    public Profissional? Profissional { get; set; }

    public bool DiaInteiro => HoraInicio is null && HoraFim is null;
}
