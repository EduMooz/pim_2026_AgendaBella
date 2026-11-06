using System.ComponentModel.DataAnnotations;

namespace AgendaBella.Modelos.Dtos;

public class ListaEsperaCriarDto
{
    [Display(Name = "Serviço")]
    [Range(1, int.MaxValue, ErrorMessage = "Escolha o serviço.")]
    public int IdServico { get; set; }

    [Display(Name = "Data desejada")]
    [Required(ErrorMessage = "Escolha a data.")]
    [DataType(DataType.Date)]
    public DateTime DataDesejada { get; set; }

    // Opcional: vazio = qualquer horário do dia.
    [Display(Name = "Horário desejado")]
    public string? HorarioDesejado { get; set; }
}

public class ListaEsperaDto
{
    public int Id { get; set; }
    public int IdServico { get; set; }
    public string ServicoNome { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;
    public string DataDesejada { get; set; } = string.Empty;
    public string? HorarioDesejado { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DataSolicitacao { get; set; }

    public static ListaEsperaDto De(ListaEspera l) => new()
    {
        Id = l.IdListaEspera,
        IdServico = l.IdServico,
        ServicoNome = l.Servico?.Nome ?? string.Empty,
        ClienteNome = l.Cliente?.Nome ?? string.Empty,
        DataDesejada = l.DataDesejada.ToString("yyyy-MM-dd"),
        HorarioDesejado = l.HorarioDesejado?.ToString(@"hh\:mm"),
        Status = l.Status,
        DataSolicitacao = l.DataSolicitacao
    };
}
