namespace AgendaBella.Modelos;

// Expediente semanal. DiaSemana: 1 = segunda ... 7 = domingo.
public class HorarioTrabalho
{
    public int IdHorarioTrabalho { get; set; }
    public int IdProfissional { get; set; }
    public int DiaSemana { get; set; }
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFim { get; set; }

    public Profissional? Profissional { get; set; }

    public static readonly string[] NomesDias =
        { "", "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira", "Sábado", "Domingo" };

    // Propriedade só de leitura (calculada): o EF Core não cria coluna para ela.
    public string NomeDia => DiaSemana is >= 1 and <= 7 ? NomesDias[DiaSemana] : "?";
}
