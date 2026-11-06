using AgendaBella.Modelos;
using AgendaBella.Modelos.Dtos;
using AgendaBella.Negocio;
using AgendaBella.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBella.Web.Controllers;

// Painel da profissional. Todas as actions exigem o perfil "Profissional".
[Authorize(Roles = Perfis.Profissional)]
public class AdminController : BaseController
{
    private readonly AgendamentoNegocio _agendamentos;
    private readonly RelatorioNegocio _relatorios;
    private readonly ListaEsperaNegocio _lista;
    private readonly ClienteNegocio _clientes;
    private readonly PontosNegocio _pontos;

    public AdminController(
        AgendamentoNegocio agendamentos, RelatorioNegocio relatorios, ListaEsperaNegocio lista,
        ClienteNegocio clientes, PontosNegocio pontos)
    {
        _agendamentos = agendamentos;
        _relatorios = relatorios;
        _lista = lista;
        _clientes = clientes;
        _pontos = pontos;
    }

    public IActionResult Dashboard() => View(new DashboardViewModel
    {
        Resumo = _relatorios.ObterResumoPainel(),
        Proximos = _agendamentos.ListarProximos(15).Select(AgendamentoDto.De).ToList()
    });

    // ---------- Agenda do dia ----------
    public IActionResult Agenda(DateTime? data)
    {
        var dia = (data ?? DateTime.Today).Date;
        var itens = _agendamentos.ListarDoDia(dia)
            .Select(a => new AgendaItemViewModel
            {
                Agendamento = AgendamentoDto.De(a),
                Preferencia = a.Cliente?.Preferencia is null ? null : PreferenciaDto.De(a.Cliente.Preferencia)
            })
            .ToList();
        return View(new AgendaDiariaViewModel { Data = dia, Itens = itens });
    }

    // ---------- Ações sobre um agendamento ----------
    // "voltar" diz para qual tela retornar (Dashboard ou Agenda); "data" mantém o dia da agenda.
    [HttpPost]
    public IActionResult Confirmar(int id, string voltar = "Dashboard", DateTime? data = null) =>
        Tentar(() => _agendamentos.Confirmar(id), "Agendamento confirmado.", Destino(voltar), null, Rota(voltar, data));

    [HttpPost]
    public IActionResult Concluir(int id, string voltar = "Dashboard", DateTime? data = null) =>
        Tentar(() => _agendamentos.Concluir(id), "Atendimento concluído. Pontos creditados à cliente.", Destino(voltar), null, Rota(voltar, data));

    [HttpPost]
    public IActionResult Cancelar(int id, string voltar = "Dashboard", DateTime? data = null) =>
        Tentar(() => _agendamentos.Cancelar(id, 0, ehProfissional: true), "Agendamento cancelado.", Destino(voltar), null, Rota(voltar, data));

    // Só aceita os dois destinos conhecidos (ignora qualquer outro texto vindo do formulário).
    private static string Destino(string voltar) => voltar == nameof(Agenda) ? nameof(Agenda) : nameof(Dashboard);

    // Só a tela "Agenda" precisa saber a data.
    private static object? Rota(string voltar, DateTime? data) =>
        voltar == nameof(Agenda) && data is not null ? new { data = data.Value.ToString("yyyy-MM-dd") } : null;

    // ---------- Lista de espera ----------
    public IActionResult ListaEspera() =>
        View(_lista.ListarAtivas().Select(ListaEsperaDto.De).ToList());

    // ---------- Clientes e pontos ----------
    public IActionResult Clientes()
    {
        var linhas = _clientes.Listar().Select(c => new ClienteLinhaViewModel
        {
            Id = c.IdCliente, Nome = c.Nome, Email = c.Email, Telefone = c.Telefone,
            Saldo = _pontos.Obter(c.IdCliente).Saldo
        }).ToList();
        return View(linhas);
    }

    [HttpPost]
    public IActionResult ResgatarPontos(ResgatarPontosDto dto) =>
        Tentar(() => _pontos.Resgatar(dto), "Pontos resgatados.", nameof(Clientes));

    // ---------- Relatórios (stored procedures + previsão) ----------
    public IActionResult Relatorios(DateTime? inicio, DateTime? fim)
    {
        var hoje = DateTime.Today;
        var modelo = new RelatorioViewModel
        {
            Inicio = (inicio ?? hoje.AddDays(-56)).Date,
            Fim = (fim ?? hoje).Date
        };

        try
        {
            modelo.Dados = _relatorios.Gerar(modelo.Inicio, modelo.Fim);
        }
        catch (RegraDeNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        return View(modelo);
    }
}
