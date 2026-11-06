using System.Security.Claims;
using AgendaBella.Modelos;

namespace AgendaBella.Web.Helpers;

// Métodos de extensão: permitem escrever  User.IdUsuario()  em qualquer controller/view.
public static class UsuarioExtensions
{
    // Id gravado no cookie no momento do login (id_cliente ou id_profissional).
    public static int IdUsuario(this ClaimsPrincipal usuario) =>
        int.TryParse(usuario.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public static bool EhProfissional(this ClaimsPrincipal usuario) => usuario.IsInRole(Perfis.Profissional);
}
