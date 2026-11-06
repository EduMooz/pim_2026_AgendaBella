namespace AgendaBella.Modelos;

// Preenchida pela trigger TR_Agendamento_Historico. O C# só lê.
public class HistoricoAgendamento
{
    public int IdHistorico { get; set; }
    public int IdAgendamento { get; set; }
    public string TipoEvento { get; set; } = string.Empty;   // Criacao | MudancaStatus | Reagendamento
    public string? StatusAnterior { get; set; }
    public string StatusNovo { get; set; } = string.Empty;
    public DateTime? DataAnterior { get; set; }
    public TimeSpan? HorarioAnterior { get; set; }
    public DateTime DataNova { get; set; }
    public TimeSpan HorarioNovo { get; set; }
    public DateTime DataEvento { get; set; }
}
