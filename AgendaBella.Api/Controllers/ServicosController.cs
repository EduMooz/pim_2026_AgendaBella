using AgendaBella.Modelos.Dtos;
using AgendaBella.Negocio;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBella.Api.Controllers;

// Catálogo público de serviços (não exige login: aparece na tela inicial do app).
[ApiController]
[Route("api/servicos")]
public class ServicosController : ControllerBase
{
    private readonly ServicoNegocio _negocio;
    public ServicosController(ServicoNegocio negocio) => _negocio = negocio;

    // GET api/servicos
    [HttpGet]
    public ActionResult<IEnumerable<ServicoDto>> Listar() =>
        Ok(_negocio.Listar(somenteAtivos: true).Select(ServicoDto.De));

    // GET api/servicos/3
    [HttpGet("{id:int}")]
    public ActionResult<ServicoDto> Obter(int id)
    {
        var servico = _negocio.ObterPorId(id);
        if (servico is null || !servico.Ativo) return NotFound();
        return Ok(ServicoDto.De(servico));
    }
}
