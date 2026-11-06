using AgendaBella.Api.Seguranca;
using AgendaBella.Modelos;
using AgendaBella.Modelos.Dtos;
using AgendaBella.Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBella.Api.Controllers;

// Preferências de unhas e pontos de fidelidade da cliente logada.
[ApiController]
[Route("api/perfil")]
[Authorize(Roles = Perfis.Cliente)]
public class PerfilController : ControllerBase
{
    private readonly ClienteNegocio _clientes;
    private readonly PontosNegocio _pontos;

    public PerfilController(ClienteNegocio clientes, PontosNegocio pontos)
    {
        _clientes = clientes;
        _pontos = pontos;
    }

    // GET api/perfil/preferencias
    [HttpGet("preferencias")]
    public ActionResult<PreferenciaDto> ObterPreferencias() =>
        Ok(PreferenciaDto.De(_clientes.ObterPreferencia(User.IdUsuario())));

    // PUT api/perfil/preferencias   -> 204
    [HttpPut("preferencias")]
    public IActionResult SalvarPreferencias(PreferenciaDto dto)
    {
        _clientes.SalvarPreferencia(User.IdUsuario(), dto);
        return NoContent();
    }

    // GET api/perfil/pontos   -> { saldo, extrato: [...] }
    [HttpGet("pontos")]
    public ActionResult<SaldoPontosDto> Pontos() => Ok(_pontos.Obter(User.IdUsuario()));
}
