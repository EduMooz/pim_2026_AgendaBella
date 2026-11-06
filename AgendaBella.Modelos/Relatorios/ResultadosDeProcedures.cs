namespace AgendaBella.Modelos.Relatorios;

// Classes que recebem o resultado das stored procedures (lidas pelo Dapper).
// Os nomes das propriedades são IGUAIS aos nomes das colunas devolvidas pela procedure.

public class HorarioDisponivel            // sp_Horarios_Disponiveis
{
    public int IdHorario { get; set; }
    public TimeSpan Horario { get; set; }
}

public class ResumoServico                // sp_Relatorio_Servicos_Periodo
{
    public int IdServico { get; set; }
    public string Servico { get; set; } = string.Empty;
    public int TotalAgendamentos { get; set; }
    public int Concluidos { get; set; }
    public int Cancelados { get; set; }
    public decimal Faturamento { get; set; }
}

public class DemandaSemanal               // sp_Relatorio_Demanda_Semanal
{
    public DateTime InicioSemana { get; set; }
    public int Total { get; set; }
}

public class DemandaDiaSemana             // sp_Relatorio_Demanda_DiaSemana
{
    public int DiaSemana { get; set; }
    public int Total { get; set; }
}

// Resultado da previsão calculada no C# (regressão linear simples).
public class PrevisaoDemanda
{
    public int SemanasAnalisadas { get; set; }
    public bool DadosSuficientes { get; set; }
    public double Inclinacao { get; set; }        // quanto a demanda muda por semana
    public double Intercepto { get; set; }
    public double R2 { get; set; }                // 0..1: quanto a reta explica os dados
    public int PrevisaoProximaSemana { get; set; }
    public string Tendencia { get; set; } = "Dados insuficientes";
}

public class RelatorioGerencial
{
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
    public List<ResumoServico> Servicos { get; set; } = new();
    public int TotalAgendamentos { get; set; }
    public int TotalConcluidos { get; set; }
    public int TotalCancelados { get; set; }
    public decimal Faturamento { get; set; }
    public double TaxaCancelamento { get; set; }      // percentual 0..100
    public string DiaDePico { get; set; } = "-";
    public List<DemandaDiaSemana> DemandaPorDia { get; set; } = new();
    public List<DemandaSemanal> DemandaSemanal { get; set; } = new();
    public PrevisaoDemanda Previsao { get; set; } = new();
}

public class ResumoPainel                  // cartões do Dashboard
{
    public int ClientesAtivos { get; set; }
    public int AgendamentosHoje { get; set; }
    public int Pendentes { get; set; }
    public int ListaEsperaAguardando { get; set; }
}
