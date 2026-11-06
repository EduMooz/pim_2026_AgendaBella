using AgendaBella.Api.Seguranca;
using AgendaBella.Modelos.Dtos;
using AgendaBella.Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBella.Api.Controllers;

// Login e cadastro pelo aplicativo. Devolve o TOKEN que o app usa nas próximas chamadas.
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AutenticacaoNegocio _autenticacao;
    private readonly GeradorDeToken _tokens;

    public AuthController(AutenticacaoNegocio autenticacao, GeradorDeToken tokens)
    {
        _autenticacao = autenticacao;
        _tokens = tokens;
    }

    // POST api/auth/login   -> 200 { token, expiraEm, usuario }  ou  401
    [HttpPost("login")]
    public ActionResult<TokenDto> Login(LoginDto dto)
    {
        var usuario = _autenticacao.Autenticar(dto.Email, dto.Senha);
        if (usuario is null)
            return Unauthorized(new { erro = "Usuário não cadastrado ou senha incorreta." });

        return Ok(_tokens.Gerar(usuario));
    }

    // POST api/auth/cadastro   -> 201 com o token (a cliente já entra logada)  ou  400
    [HttpPost("cadastro")]
    public ActionResult<TokenDto> Cadastro(CadastroClienteDto dto)
    {
        var cliente = _autenticacao.CadastrarCliente(dto);   // regra violada -> filtro devolve 400
        var usuario = new UsuarioLogadoDto
        {
            Id = cliente.IdCliente, Nome = cliente.Nome, Email = cliente.Email, Perfil = cliente.TipoUsuario()
        };
        return StatusCode(StatusCodes.Status201Created, _tokens.Gerar(usuario));
    }

    // GET api/auth/eu   -> quem é o dono do token (o app usa para conferir se o token ainda vale)
    [Authorize]
    [HttpGet("eu")]
    public ActionResult<UsuarioLogadoDto> Eu() => Ok(new UsuarioLogadoDto
    {
        Id = User.IdUsuario(),
        Nome = User.FindFirst("name")?.Value ?? string.Empty,
        Email = User.FindFirst("email")?.Value ?? string.Empty,
        Perfil = User.FindFirst("role")?.Value ?? string.Empty
    });
}
