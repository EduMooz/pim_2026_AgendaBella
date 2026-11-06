using System.ComponentModel.DataAnnotations;
using AgendaBella.Modelos.Relatorios;

namespace AgendaBella.Modelos.Dtos;

// ENTRADA: nova marcação. A Data chega como yyyy-MM-dd e o Horario como "HH:mm".
public class AgendamentoCriarDto
{
    [Display(Name = "Serviço")]
    [Range(1, int.MaxValue, ErrorMessage = "Escolha o serviço.")]
    public int IdServico { get; set; }

    [Display(Name = "Data")]
    [Required(ErrorMessage = "Escolha a data.")]
    [DataType(DataType.Date)]
    public DateTime Data { get; set; }

    [Display(Name = "Horário")]
    [Required(ErrorMessage = "Escolha o horário.")]
    public string Horario { get; set; } = string.Empty;

    [Display(Name = "Observação")]
    [StringLength(500, ErrorMessage = "A observação pode ter no máximo 500 caracteres.")]
    public string? Observacao { get; set; }
}

// ENTRADA: novo dia/horário para um agendamento existente.
public class ReagendarDto
{
    [Display(Name = "Data")]
    [Required(ErrorMessage = "Escolha a data.")]
    [DataType(DataType.Date)]
    public DateTime Data { get; set; }

    [Display(Name = "Horário")]
    [Required(ErrorMessage = "Escolha o horário.")]
    public string Horario { get; set; } = string.Empty;
}

// SAÍDA: agendamento já "traduzido" para tela/app (strings prontas de data e hora).
public class AgendamentoDto
{
    public int Id { get; set; }
    public int IdCliente { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string? ClienteTelefone { get; set; }
    public int IdServico { get; set; }
    public string ServicoNome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int Duracao { get; set; }
    public string Data { get; set; } = string.Empty;       // yyyy-MM-dd
    public string Horario { get; set; } = string.Empty;    // HH:mm
    public string Status { get; set; } = string.Empty;
    public string? ImagemInspiracao { get; set; }          // caminho relativo, ex.: inspiracao/abc.jpg
    public string? Observacao { get; set; }
    public bool PodeCancelar { get; set; }
    public bool PodeReagendar { get; set; }

    public static AgendamentoDto De(Agendamento a)
    {
        // Mesmo critério da regra de negócio: ativo e com a antecedência mínima.
        var comAntecedencia = a.EstaAtivo &&
            a.Inicio - DateTime.Now >= TimeSpan.FromHours(RegrasAgendamento.AntecedenciaMinimaHoras);

        return new AgendamentoDto
        {
            Id = a.IdAgendamento,
            IdCliente = a.IdCliente,
            ClienteNome = a.Cliente?.Nome ?? string.Empty,
            ClienteTelefone = a.Cliente?.Telefone,
            IdServico = a.IdServico,
            ServicoNome = a.Servico?.Nome ?? string.Empty,
            Preco = a.Servico?.Preco ?? 0,
            Duracao = a.Servico?.Duracao ?? 0,
            Data = a.DataAgendamento.ToString("yyyy-MM-dd"),
            Horario = a.Horario.ToString(@"hh\:mm"),
            Status = a.Status,
            ImagemInspiracao = a.ImagemInspiracao,
            Observacao = a.Observacao,
            PodeCancelar = comAntecedencia,
            PodeReagendar = comAntecedencia
        };
    }
}

// SAÍDA: um horário livre.
public class HorarioDisponivelDto
{
    public int IdHorario { get; set; }
    public string Horario { get; set; } = string.Empty;    // HH:mm

    public static HorarioDisponivelDto De(HorarioDisponivel h) => new()
    {
        IdHorario = h.IdHorario,
        Horario = h.Horario.ToString(@"hh\:mm")
    };
}

// SAÍDA: uma linha do histórico do agendamento (vinda da trigger).
public class HistoricoAgendamentoDto
{
    public string Evento { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataEvento { get; set; }

    public static HistoricoAgendamentoDto De(HistoricoAgendamento h)
    {
        var descricao = h.TipoEvento switch
        {
            "Criacao" => $"Agendamento criado para {h.DataNova:dd/MM/yyyy} às {h.HorarioNovo:hh\\:mm}.",
            "Reagendamento" => $"Reagendado de {h.DataAnterior:dd/MM/yyyy} {h.HorarioAnterior:hh\\:mm} para {h.DataNova:dd/MM/yyyy} {h.HorarioNovo:hh\\:mm}.",
            _ => $"Status alterado de {h.StatusAnterior} para {h.StatusNovo}."
        };
        var titulo = h.TipoEvento switch
        {
            "Criacao" => "Criação",
            "Reagendamento" => "Reagendamento",
            _ => "Mudança de status"
        };
        return new HistoricoAgendamentoDto { Evento = titulo, Descricao = descricao, DataEvento = h.DataEvento };
    }
}
