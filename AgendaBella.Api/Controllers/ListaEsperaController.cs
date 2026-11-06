using AgendaBella.Api.Seguranca;
using AgendaBella.Modelos;
using AgendaBella.Modelos.Dtos;
using AgendaBella.Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBella.Api.Controllers;

// Lista de espera: a funcionalidade complementar do aplicativo.
// Quando uma vaga abre, a TRIGGER muda o status para "Notificado" e o app mostra o aviso.
[ApiController]
[Route("api/lista-espera")]
[Authorize(Roles = Perfis.Cliente)]
public class ListaEsperaController : ControllerBase
{
    private readonly ListaEsperaNegocio _negocio;
    public ListaEsperaController(ListaEsperaNegocio negocio) => _negocio = negocio;

    // GET api/lista-espera
    [HttpGet]
    public ActionResult<IEnumerable<ListaEsperaDto>> Listar() =>
        Ok(_negocio.ListarDoCliente(User.IdUsuario()).Select(ListaEsperaDto.De));

    // POST api/lista-espera   { "idServico": 4, "dataDesejada": "2026-10-20", "horarioDesejado": null }  -> 201
    [HttpPost]
    public ActionResult<ListaEsperaDto> Entrar(ListaEsperaCriarDto dto)
    {
        var item = _negocio.Entrar(User.IdUsuario(), dto);
        var completo = _negocio.ListarDoCliente(User.IdUsuario()).First(l => l.IdListaEspera == item.IdListaEspera);
        return StatusCode(StatusCodes.Status201Created, ListaEsperaDto.De(completo));
    }

    // DELETE api/lista-espera/7   -> 204 (sai da fila; o registro fica como Cancelado)
    [HttpDelete("{id:int}")]
    public IActionResult Sair(int id)
    {
        _negocio.Sair(id, User.IdUsuario());
        return NoContent();
    }
}
