using System.ComponentModel.DataAnnotations;

namespace AgendaBella.Modelos.Dtos;

public class HorarioTrabalhoDto
{
    public int Id { get; set; }
    public int DiaSemana { get; set; }
    public string NomeDia { get; set; } = string.Empty;
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFim { get; set; } = string.Empty;

    public static HorarioTrabalhoDto De(HorarioTrabalho h) => new()
    {
        Id = h.IdHorarioTrabalho, DiaSemana = h.DiaSemana, NomeDia = h.NomeDia,
        HoraInicio = h.HoraInicio.ToString(@"hh\:mm"), HoraFim = h.HoraFim.ToString(@"hh\:mm")
    };
}

public class HorarioTrabalhoSalvarDto
{
    [Display(Name = "Dia da semana")]
    [Range(1, 7, ErrorMessage = "Escolha o dia da semana.")]
    public int DiaSemana { get; set; } = 1;

    [Display(Name = "Início")]
    [Required(ErrorMessage = "Informe o horário de início.")]
    public string HoraInicio { get; set; } = "09:00";

    [Display(Name = "Fim")]
    [Required(ErrorMessage = "Informe o horário de fim.")]
    public string HoraFim { get; set; } = "18:00";
}

public class BloqueioDto
{
    public int Id { get; set; }
    public string Data { get; set; } = string.Empty;
    public string? HoraInicio { get; set; }
    public string? HoraFim { get; set; }
    public string? Motivo { get; set; }
    public bool DiaInteiro { get; set; }

    public static BloqueioDto De(BloqueioHorario b) => new()
    {
        Id = b.IdBloqueio,
        Data = b.DataBloqueio.ToString("yyyy-MM-dd"),
        HoraInicio = b.HoraInicio?.ToString(@"hh\:mm"),
        HoraFim = b.HoraFim?.ToString(@"hh\:mm"),
        Motivo = b.Motivo,
        DiaInteiro = b.DiaInteiro
    };
}

public class BloqueioSalvarDto
{
    [Display(Name = "Data")]
    [Required(ErrorMessage = "Escolha a data.")]
    [DataType(DataType.Date)]
    public DateTime Data { get; set; } = DateTime.Today;

    // Os dois vazios = dia inteiro.
    [Display(Name = "Início")]
    public string? HoraInicio { get; set; }

    [Display(Name = "Fim")]
    public string? HoraFim { get; set; }

    [Display(Name = "Motivo")]
    [StringLength(255, ErrorMessage = "O motivo pode ter no máximo 255 caracteres.")]
    public string? Motivo { get; set; }
}
