namespace AgendaBella.Modelos;

// Um horário de INÍCIO da grade (09:00, 09:30, ...). Usado pela procedure sp_Horarios_Disponiveis.
public class HorarioAgendamento
{
    public int IdHorario { get; set; }
    public TimeSpan Horario { get; set; }
}
