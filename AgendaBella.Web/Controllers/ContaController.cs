using System.Security.Claims;
using AgendaBella.Modelos;
using AgendaBella.Modelos.Dtos;
using AgendaBella.Negocio;
using AgendaBella.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBella.Web.Controllers;

public class ContaController : Controller
{
    private readonly AutenticacaoNegocio _autenticacao;

    public ContaController(AutenticacaoNegocio autenticacao) => _autenticacao = autenticacao;

    // GET /Conta/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginPaginaViewModel { ReturnUrl = returnUrl });

    // POST /Conta/Login  (o Prefix liga os campos "Login.Email" e "Login.Senha" ao LoginDto)
    [HttpPost]
    public async Task<IActionResult> Login([Bind(Prefix = "Login")] LoginDto dto, string? returnUrl)
    {
        var pagina = new LoginPaginaViewModel { Login = dto, ReturnUrl = returnUrl };
        if (!ModelState.IsValid) return View(pagina);

        var usuario = _autenticacao.Autenticar(dto.Email, dto.Senha);
        if (usuario is null)
        {
            ModelState.AddModelError(string.Empty, "Usuário não cadastrado ou senha incorreta.");
            return View(pagina);
        }

        // Guarda quem é a pessoa dentro do cookie (as "claims").
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Perfil)
        };
        var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidade));

        // Só redireciona para endereços do próprio site (evita "open redirect").
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);

        return usuario.Perfil == Perfis.Profissional
            ? RedirectToAction("Dashboard", "Admin")
            : RedirectToAction("Index", "Home");
    }

    // POST /Conta/Cadastro
    [HttpPost]
    public IActionResult Cadastro([Bind(Prefix = "Cadastro")] CadastroClienteDto dto)
    {
        var pagina = new LoginPaginaViewModel { Cadastro = dto, AbrirCadastro = true };
        if (!ModelState.IsValid) return View("Login", pagina);

        try
        {
            _autenticacao.CadastrarCliente(dto);
            return RedirectToAction(nameof(CadastroConfirmado));
        }
        catch (RegraDeNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Login", pagina);
        }
    }

    public IActionResult CadastroConfirmado() => View();

    [HttpGet]
    public IActionResult EsqueciASenha() => View(new RedefinirSenhaDto());

    [HttpPost]
    public IActionResult EsqueciASenha(RedefinirSenhaDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            _autenticacao.RedefinirSenha(dto);
            TempData["Mensagem"] = "Senha redefinida com sucesso. Entre com a nova senha.";
            return RedirectToAction(nameof(Login));
        }
        catch (RegraDeNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    // Logout é POST (e protegido por token) para que ninguém desconecte a pessoa com um link malicioso.
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AcessoNegado() => View();
}
