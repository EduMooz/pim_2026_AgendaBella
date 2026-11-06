using AgendaBella.Modelos;
using AgendaBella.Modelos.Dtos;
using AgendaBella.Negocio;
using AgendaBella.Web.Helpers;
using AgendaBella.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBella.Web.Controllers;

// Área da cliente. [Authorize(Roles = ...)] barra quem não está logada (vai para o login)
// e quem tem outro perfil (vai para "Acesso negado").
[Authorize(Roles = Perfis.Cliente)]
public class AgendamentoController : Controller
{
    private readonly AgendamentoNegocio _agendamentos;
    private readonly ServicoNegocio _servicos;
    private readonly PontosNegocio _pontos;
    private readonly ImagemInspiracaoNegocio _imagens;
    private readonly PastaUploads _pasta;

    public AgendamentoController(
        AgendamentoNegocio agendamentos, ServicoNegocio servicos, PontosNegocio pontos,
        ImagemInspiracaoNegocio imagens, PastaUploads pasta)
    {
        _agendamentos = agendamentos;
        _servicos = servicos;
        _pontos = pontos;
        _imagens = imagens;
        _pasta = pasta;
    }

    private List<ServicoDto> ServicosAtivos() =>
        _servicos.Listar(somenteAtivos: true).Select(ServicoDto.De).ToList();

    // ---------- Novo agendamento ----------

    // GET /Agendamento
    public IActionResult Index(int? servico = null)
    {
        var modelo = new AgendamentoNovoViewModel { Servicos = ServicosAtivos() };
        if (servico is not null) modelo.Dados.IdServico = servico.Value;
        return View(modelo);
    }

    // GET /Agendamento/Horarios?data=2026-10-10&idServico=3  -> JSON usado pelo JavaScript da tela.
    // É AQUI que a stored procedure sp_Horarios_Disponiveis é executada (via Negocio -> Repositorio).
    [HttpGet]
    public IActionResult Horarios(DateTime data, int idServico, int? ignorar = null)
    {
        // "ignorar" só vale se o agendamento for MESMO da cliente logada.
        if (ignorar is not null && _agendamentos.Obter(ignorar.Value, User.IdUsuario(), false) is null)
            ignorar = null;

        var livres = _agendamentos.ListarHorariosDisponiveis(data, idServico, ignorar);
        return Json(livres.Select(HorarioDisponivelDto.De));
    }

    // POST /Agendamento/Criar  (formulário com upload: enctype multipart/form-data)
    // O Prefix "Dados" liga os campos Dados.IdServico, Dados.Data... ao DTO.
    [HttpPost]
    public async Task<IActionResult> Criar([Bind(Prefix = "Dados")] AgendamentoCriarDto dto, IFormFile? imagem)
    {
        if (!ModelState.IsValid)
            return View("Index", new AgendamentoNovoViewModel { Servicos = ServicosAtivos(), Dados = dto });

        var idCliente = User.IdUsuario();
        try
        {
            var agendamento = _agendamentos.Criar(idCliente, dto);

            // A imagem é anexada DEPOIS de o agendamento existir: se o horário falhar, nenhuma imagem sobra no disco.
            if (imagem is { Length: > 0 })
            {
                string? caminho = null;
                try
                {
                    await using var fluxo = imagem.OpenReadStream();
                    caminho = await _imagens.SalvarAsync(fluxo, imagem.FileName, imagem.Length, _pasta.Caminho);
                    _agendamentos.AnexarImagem(agendamento.IdAgendamento, idCliente, caminho);
                }
                catch (RegraDeNegocioException ex)
                {
                    _imagens.Remover(caminho, _pasta.Caminho);
                    TempData["Erro"] = "Agendamento criado, mas a imagem não foi anexada: " + ex.Message;
                }
            }

            return RedirectToAction(nameof(Confirmacao), new { id = agendamento.IdAgendamento });
        }
        catch (RegraDeNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Index", new AgendamentoNovoViewModel { Servicos = ServicosAtivos(), Dados = dto });
        }
    }

    public IActionResult Confirmacao(int id)
    {
        var ag = _agendamentos.Obter(id, User.IdUsuario(), false);
        return ag is null ? NotFound() : View(AgendamentoDto.De(ag));
    }

    // ---------- Meus agendamentos ----------

    public IActionResult Meus()
    {
        var idCliente = User.IdUsuario();
        var modelo = new MeusAgendamentosViewModel
        {
            Agendamentos = _agendamentos.ListarDoCliente(idCliente).Select(AgendamentoDto.De).ToList(),
            Saldo = _pontos.Obter(idCliente).Saldo
        };
        return View(modelo);
    }

    [HttpPost]
    public IActionResult Cancelar(int id)
    {
        try
        {
            _agendamentos.Cancelar(id, User.IdUsuario(), ehProfissional: false);
            TempData["Mensagem"] = "Agendamento cancelado.";
        }
        catch (RegraDeNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }
        return RedirectToAction(nameof(Meus));
    }

    // ---------- Reagendar ----------

    [HttpGet]
    public IActionResult Reagendar(int id)
    {
        var ag = _agendamentos.Obter(id, User.IdUsuario(), false);
        if (ag is null) return NotFound();
        return View(new ReagendarViewModel { Agendamento = AgendamentoDto.De(ag) });
    }

    [HttpPost]
    public IActionResult Reagendar(int id, [Bind(Prefix = "Dados")] ReagendarDto dados)
    {
        var ag = _agendamentos.Obter(id, User.IdUsuario(), false);
        if (ag is null) return NotFound();
        var modelo = new ReagendarViewModel { Agendamento = AgendamentoDto.De(ag), Dados = dados };

        if (!ModelState.IsValid) return View(modelo);

        try
        {
            _agendamentos.Reagendar(id, User.IdUsuario(), false, dados);
            TempData["Mensagem"] = "Agendamento reagendado.";
            return RedirectToAction(nameof(Meus));
        }
        catch (RegraDeNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    // ---------- Histórico (preenchido pela trigger) ----------

    public IActionResult Historico(int id)
    {
        var ag = _agendamentos.Obter(id, User.IdUsuario(), false);
        if (ag is null) return NotFound();

        var eventos = _agendamentos.ListarHistorico(id, User.IdUsuario(), false)
            .Select(HistoricoAgendamentoDto.De).ToList();
        return View(new HistoricoViewModel { Agendamento = AgendamentoDto.De(ag), Eventos = eventos });
    }
}
