using AgendaBella.Modelos.Dtos;
using AgendaBella.Modelos.Relatorios;

namespace AgendaBella.Web.ViewModels;

// ViewModel = objeto montado só para uma tela, juntando o que ela precisa mostrar.
// Os controllers montam o ViewModel; a view só exibe.

public class LoginPaginaViewModel
{
    public LoginDto Login { get; set; } = new();
    public CadastroClienteDto Cadastro { get; set; } = new();
    public string? ReturnUrl { get; set; }
    public bool AbrirCadastro { get; set; }
}

public class HomeViewModel
{
    public List<ServicoDto> Servicos { get; set; } = new();
}

public class AgendamentoNovoViewModel
{
    public List<ServicoDto> Servicos { get; set; } = new();
    public AgendamentoCriarDto Dados { get; set; } = new();
}

public class ReagendarViewModel
{
    public AgendamentoDto Agendamento { get; set; } = new();
    public ReagendarDto Dados { get; set; } = new();
}

public class MeusAgendamentosViewModel
{
    public List<AgendamentoDto> Agendamentos { get; set; } = new();
    public int Saldo { get; set; }
}

public class HistoricoViewModel
{
    public AgendamentoDto Agendamento { get; set; } = new();
    public List<HistoricoAgendamentoDto> Eventos { get; set; } = new();
}

public class ListaEsperaViewModel
{
    public List<ServicoDto> Servicos { get; set; } = new();
    public ListaEsperaCriarDto Dados { get; set; } = new();
    public List<ListaEsperaDto> Minhas { get; set; } = new();
}

public class DashboardViewModel
{
    public ResumoPainel Resumo { get; set; } = new();
    public List<AgendamentoDto> Proximos { get; set; } = new();
}

public class AgendaItemViewModel
{
    public AgendamentoDto Agendamento { get; set; } = new();
    public PreferenciaDto? Preferencia { get; set; }
}

public class AgendaDiariaViewModel
{
    public DateTime Data { get; set; }
    public List<AgendaItemViewModel> Itens { get; set; } = new();
}

public class HorariosAdminViewModel
{
    public List<HorarioTrabalhoDto> Horarios { get; set; } = new();
    public List<BloqueioDto> Bloqueios { get; set; } = new();
    public HorarioTrabalhoSalvarDto NovoHorario { get; set; } = new();
    public BloqueioSalvarDto NovoBloqueio { get; set; } = new();
}

public class RelatorioViewModel
{
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
    public RelatorioGerencial? Dados { get; set; }
}

public class ClienteLinhaViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public int Saldo { get; set; }
}

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
