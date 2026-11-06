using AgendaBella.Api.Seguranca;
using AgendaBella.Modelos;
using AgendaBella.Modelos.Dtos;
using AgendaBella.Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBella.Api.Controllers;

// Endpoints REST dos agendamentos da cliente logada (perfil "Cliente" dentro do token).
// O controller só recebe a requisição, chama o Negocio e escolhe o código HTTP.
[ApiController]
[Route("api/agendamentos")]
[Authorize(Roles = Perfis.Cliente)]
public class AgendamentosController : ControllerBase
{
    private readonly AgendamentoNegocio _negocio;
    private readonly ImagemInspiracaoNegocio _imagens;
    private readonly PastaUploadsApi _pasta;

    public AgendamentosController(AgendamentoNegocio negocio, ImagemInspiracaoNegocio imagens, PastaUploadsApi pasta)
    {
        _negocio = negocio;
        _imagens = imagens;
        _pasta = pasta;
    }

    // GET api/agendamentos   -> os agendamentos da cliente (mais recentes primeiro)
    [HttpGet]
    public ActionResult<IEnumerable<AgendamentoDto>> Listar() =>
        Ok(_negocio.ListarDoCliente(User.IdUsuario()).Select(AgendamentoDto.De));

    // GET api/agendamentos/5
    [HttpGet("{id:int}")]
    public ActionResult<AgendamentoDto> Obter(int id)
    {
        var ag = _negocio.Obter(id, User.IdUsuario(), ehProfissional: false);
        return ag is null ? NotFound() : Ok(AgendamentoDto.De(ag));
    }

    // GET api/agendamentos/horarios?data=2026-10-15&idServico=3[&ignorar=5]
    // Executa a STORED PROCEDURE sp_Horarios_Disponiveis (Negocio -> Repositorio -> Dapper).
    [HttpGet("horarios")]
    public ActionResult<IEnumerable<HorarioDisponivelDto>> Horarios(
        [FromQuery] DateTime data, [FromQuery] int idServico, [FromQuery] int? ignorar = null)
    {
        if (ignorar is not null && _negocio.Obter(ignorar.Value, User.IdUsuario(), false) is null)
            ignorar = null;   // só pode ignorar um agendamento que é dela

        return Ok(_negocio.ListarHorariosDisponiveis(data, idServico, ignorar).Select(HorarioDisponivelDto.De));
    }

    // POST api/agendamentos   -> 201 Created + cabeçalho Location  ou  400
    [HttpPost]
    public ActionResult<AgendamentoDto> Criar(AgendamentoCriarDto dto)
    {
        var criado = _negocio.Criar(User.IdUsuario(), dto);
        // Recarrega para trazer o nome do serviço e da cliente no DTO de resposta.
        var completo = _negocio.Obter(criado.IdAgendamento, User.IdUsuario(), false) ?? criado;
        return CreatedAtAction(nameof(Obter), new { id = criado.IdAgendamento }, AgendamentoDto.De(completo));
    }

    // POST api/agendamentos/5/imagem   (multipart/form-data, campo "imagem")
    [HttpPost("{id:int}/imagem")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<AgendamentoDto>> EnviarImagem(int id, IFormFile imagem)
    {
        if (_negocio.Obter(id, User.IdUsuario(), false) is null) return NotFound();

        string? caminho = null;
        try
        {
            await using var fluxo = imagem.OpenReadStream();
            caminho = await _imagens.SalvarAsync(fluxo, imagem.FileName, imagem.Length, _pasta.Caminho);
            _negocio.AnexarImagem(id, User.IdUsuario(), caminho);
        }
        catch (RegraDeNegocioException)
        {
            _imagens.Remover(caminho, _pasta.Caminho);   // não deixa arquivo "órfão" no disco
            throw;                                       // o filtro devolve 400 com a mensagem
        }

        return Ok(AgendamentoDto.De(_negocio.Obter(id, User.IdUsuario(), false)!));
    }

    // PATCH api/agendamentos/5/reagendar   { "data": "2026-10-20", "horario": "14:00" }   -> 204
    [HttpPatch("{id:int}/reagendar")]
    public IActionResult Reagendar(int id, ReagendarDto dto)
    {
        if (_negocio.Obter(id, User.IdUsuario(), false) is null) return NotFound();
        _negocio.Reagendar(id, User.IdUsuario(), ehProfissional: false, dto);
        return NoContent();
    }

    // PATCH api/agendamentos/5/cancelar   -> 204 (o registro continua no histórico com status Cancelado)
    [HttpPatch("{id:int}/cancelar")]
    public IActionResult Cancelar(int id)
    {
        if (_negocio.Obter(id, User.IdUsuario(), false) is null) return NotFound();
        _negocio.Cancelar(id, User.IdUsuario(), ehProfissional: false);
        return NoContent();
    }

    // GET api/agendamentos/5/historico   -> eventos gravados pela TRIGGER TR_Agendamento_Historico
    [HttpGet("{id:int}/historico")]
    public ActionResult<IEnumerable<HistoricoAgendamentoDto>> Historico(int id)
    {
        if (_negocio.Obter(id, User.IdUsuario(), false) is null) return NotFound();
        return Ok(_negocio.ListarHistorico(id, User.IdUsuario(), false).Select(HistoricoAgendamentoDto.De));
    }
}
