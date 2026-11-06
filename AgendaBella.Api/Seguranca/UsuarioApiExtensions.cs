using System.Security.Claims;

namespace AgendaBella.Api.Seguranca;

// Lê os dados do usuário de dentro do token (as claims gravadas pelo GeradorDeToken).
public static class UsuarioApiExtensions
{
    public static int IdUsuario(this ClaimsPrincipal usuario) =>
        int.TryParse(usuario.FindFirstValue("sub"), out var id) ? id : 0;
}
