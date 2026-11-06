using System.Diagnostics;
using AgendaBella.Modelos.Dtos;
using AgendaBella.Negocio;
using AgendaBella.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBella.Web.Controllers;

public class HomeController : Controller
{
    private readonly ServicoNegocio _servicos;

    public HomeController(ServicoNegocio servicos) => _servicos = servicos;

    // A página inicial lista os serviços DE VERDADE, direto do banco.
    public IActionResult Index()
    {
        var modelo = new HomeViewModel
        {
            Servicos = _servicos.Listar(somenteAtivos: true).Select(ServicoDto.De).ToList()
        };
        return View(modelo);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
