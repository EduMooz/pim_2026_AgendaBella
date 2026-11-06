using AgendaBella.Modelos;
using AgendaBella.Modelos.Dtos;
using AgendaBella.Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBella.Web.Controllers;

// CRUD de serviços (Create, Read, Update, Delete) da profissional.
[Authorize(Roles = Perfis.Profissional)]
public class ServicosController : BaseController
{
    private readonly ServicoNegocio _negocio;
    public ServicosController(ServicoNegocio negocio) => _negocio = negocio;

    public IActionResult Index() =>
        View(_negocio.Listar(somenteAtivos: false).Select(ServicoDto.De).ToList());

    [HttpGet]
    public IActionResult Criar() => View(new ServicoSalvarDto());

    [HttpPost]
    public IActionResult Criar(ServicoSalvarDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            _negocio.Cadastrar(dto);
            TempData["Mensagem"] = "Serviço cadastrado.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraDeNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    public IActionResult Editar(int id)
    {
        var servico = _negocio.ObterPorId(id);
        if (servico is null) return NotFound();
        ViewData["Id"] = id;
        return View(ServicoSalvarDto.De(servico));
    }

    [HttpPost]
    public IActionResult Editar(int id, ServicoSalvarDto dto)
    {
        ViewData["Id"] = id;
        if (!ModelState.IsValid) return View(dto);
        try
        {
            _negocio.Atualizar(id, dto);
            TempData["Mensagem"] = "Serviço atualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraDeNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpPost]
    public IActionResult Excluir(int id)
    {
        var resultado = _negocio.Remover(id);
        TempData["Mensagem"] = resultado switch
        {
            ResultadoRemocao.Removido => "Serviço excluído.",
            ResultadoRemocao.Desativado => "O serviço já foi usado em agendamentos; foi desativado para preservar o histórico.",
            _ => "Serviço não encontrado."
        };
        return RedirectToAction(nameof(Index));
    }
}
