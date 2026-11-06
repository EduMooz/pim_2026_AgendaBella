using AgendaBella.Modelos;
using AgendaBella.Modelos.Relatorios;
using AgendaBella.Repositorio.Interfaces;

namespace AgendaBella.Negocio;

// Indicadores gerenciais e previsão de demanda (Etapa 7 - Análise de Dados e Machine Learning).
public class RelatorioNegocio
{
    private const int SemanasParaPrevisao = 8;

    private readonly IRelatorioRepositorio _relatorios;
    private readonly IClienteRepositorio _clientes;
    private readonly IAgendamentoRepositorio _agendamentos;
    private readonly IListaEsperaRepositorio _listaEspera;

    public RelatorioNegocio(
        IRelatorioRepositorio relatorios,
        IClienteRepositorio clientes,
        IAgendamentoRepositorio agendamentos,
        IListaEsperaRepositorio listaEspera)
    {
        _relatorios = relatorios;
        _clientes = clientes;
        _agendamentos = agendamentos;
        _listaEspera = listaEspera;
    }

    public ResumoPainel ObterResumoPainel() => new()
    {
        ClientesAtivos = _clientes.ContarAtivos(),
        AgendamentosHoje = _agendamentos.ContarDoDia(DateTime.Today),
        Pendentes = _agendamentos.ContarPendentes(),
        ListaEsperaAguardando = _listaEspera.ContarAguardando()
    };

    public RelatorioGerencial Gerar(DateTime inicio, DateTime fim)
    {
        inicio = inicio.Date;
        fim = fim.Date;
        if (fim < inicio) throw new RegraDeNegocioException("A data final deve ser maior ou igual à inicial.");
        if ((fim - inicio).TotalDays > 366) throw new RegraDeNegocioException("O período máximo é de 1 ano.");

        var servicos = _relatorios.ResumoPorServico(inicio, fim);
        var porDia = _relatorios.DemandaPorDiaSemana(inicio, fim);
        var semanal = _relatorios.DemandaSemanal(SemanasParaPrevisao);

        var total = servicos.Sum(s => s.TotalAgendamentos);
        var cancelados = servicos.Sum(s => s.Cancelados);
        var pico = porDia.OrderByDescending(d => d.Total).FirstOrDefault();

        return new RelatorioGerencial
        {
            Inicio = inicio,
            Fim = fim,
            Servicos = servicos,
            TotalAgendamentos = total,
            TotalConcluidos = servicos.Sum(s => s.Concluidos),
            TotalCancelados = cancelados,
            Faturamento = servicos.Sum(s => s.Faturamento),
            TaxaCancelamento = total == 0 ? 0 : Math.Round(cancelados * 100.0 / total, 1),
            DiaDePico = pico is { Total: > 0 } ? HorarioTrabalho.NomesDias[pico.DiaSemana] : "-",
            DemandaPorDia = porDia,
            DemandaSemanal = semanal,
            Previsao = CalcularPrevisao(semanal)
        };
    }

    // REGRESSÃO LINEAR SIMPLES (mínimos quadrados): ajusta a reta  y = intercepto + inclinação * x
    // onde x = número da semana (0,1,2,...) e y = quantidade de agendamentos.
    // Prevê a próxima semana (x = n). É aprendizado supervisionado básico, feito à mão
    // para a equipe enxergar a matemática por trás.
    public static PrevisaoDemanda CalcularPrevisao(IReadOnlyList<DemandaSemanal> semanas)
    {
        var n = semanas.Count;
        var resultado = new PrevisaoDemanda { SemanasAnalisadas = n };
        if (n < 4) return resultado;      // poucos pontos: previsão não confiável

        double mediaX = (n - 1) / 2.0;
        double mediaY = semanas.Average(s => (double)s.Total);

        double sxy = 0, sxx = 0, sst = 0;
        for (var i = 0; i < n; i++)
        {
            var dx = i - mediaX;
            var dy = semanas[i].Total - mediaY;
            sxy += dx * dy;
            sxx += dx * dx;
            sst += dy * dy;
        }

        var inclinacao = sxy / sxx;
        var intercepto = mediaY - inclinacao * mediaX;

        double sse = 0;                    // soma dos erros ao quadrado
        for (var i = 0; i < n; i++)
        {
            var erro = semanas[i].Total - (intercepto + inclinacao * i);
            sse += erro * erro;
        }

        resultado.DadosSuficientes = true;
        resultado.Inclinacao = Math.Round(inclinacao, 3);
        resultado.Intercepto = Math.Round(intercepto, 3);
        resultado.R2 = sst == 0 ? 1 : Math.Round(1 - sse / sst, 3);
        resultado.PrevisaoProximaSemana = Math.Max(0, (int)Math.Round(intercepto + inclinacao * n));
        resultado.Tendencia = inclinacao > 0.25 ? "Alta" : inclinacao < -0.25 ? "Queda" : "Estável";
        return resultado;
    }
}
