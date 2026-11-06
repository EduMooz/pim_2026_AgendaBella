using AgendaBella.Modelos;
using AgendaBella.Modelos.Dtos;
using AgendaBella.Negocio;
using AgendaBella.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBella.Web.Controllers;

// Preferências de unhas e programa de fidelidade da cliente.
[Authorize(Roles = Perfis.Cliente)]
public class PerfilController : Controller
{
    private readonly ClienteNegocio _clientes;
    private readonly PontosNegocio _pontos;

    public PerfilController(ClienteNegocio clientes, PontosNegocio pontos)
    {
        _clientes = clientes;
        _pontos = pontos;
    }

    [HttpGet]
    public IActionResult Preferencias() =>
        View(PreferenciaDto.De(_clientes.ObterPreferencia(User.IdUsuario())));

    [HttpPost]
    public IActionResult Preferencias(PreferenciaDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            _clientes.SalvarPreferencia(User.IdUsuario(), dto);
            TempData["Mensagem"] = "Preferências salvas.";
            return RedirectToAction(nameof(Preferencias));
        }
        catch (RegraDeNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    public IActionResult Pontos() => View(_pontos.Obter(User.IdUsuario()));
}
