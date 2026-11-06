using AgendaBella.Modelos;
using AgendaBella.Modelos.Dtos;
using AgendaBella.Negocio;
using AgendaBella.Web.Helpers;
using AgendaBella.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBella.Web.Controllers;

[Authorize(Roles = Perfis.Cliente)]
public class ListaEsperaController : BaseController
{
    private readonly ListaEsperaNegocio _lista;
    private readonly ServicoNegocio _servicos;

    public ListaEsperaController(ListaEsperaNegocio lista, ServicoNegocio servicos)
    {
        _lista = lista;
        _servicos = servicos;
    }

    private ListaEsperaViewModel Montar(ListaEsperaCriarDto? dados = null) => new()
    {
        Servicos = _servicos.Listar(true).Select(ServicoDto.De).ToList(),
        Minhas = _lista.ListarDoCliente(User.IdUsuario()).Select(ListaEsperaDto.De).ToList(),
        Dados = dados ?? new ListaEsperaCriarDto { DataDesejada = DateTime.Today.AddDays(1) }
    };

    // GET /ListaEspera?servico=3&data=2026-10-10  (links vindos da tela de agendamento)
    public IActionResult Index(int? servico = null, DateTime? data = null)
    {
        var modelo = Montar();
        if (servico is not null) modelo.Dados.IdServico = servico.Value;
        if (data is not null) modelo.Dados.DataDesejada = data.Value;
        return View(modelo);
    }

    [HttpPost]
    public IActionResult Entrar([Bind(Prefix = "Dados")] ListaEsperaCriarDto dados)
    {
        if (!ModelState.IsValid) return View("Index", Montar(dados));

        try
        {
            _lista.Entrar(User.IdUsuario(), dados);
            TempData["Mensagem"] = "Pronto! Você está na lista de espera. Avisaremos quando abrir uma vaga.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraDeNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Index", Montar(dados));
        }
    }

    [HttpPost]
    public IActionResult Sair(int id) =>
        Tentar(() => _lista.Sair(id, User.IdUsuario()), "Você saiu da lista de espera.", nameof(Index));
}
