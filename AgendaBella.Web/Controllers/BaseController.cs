using AgendaBella.Negocio;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBella.Web.Controllers;

// Controller-base com um utilitário usado por várias telas: executa uma ação de negócio e
// transforma o resultado em mensagem para a tela (TempData sobrevive ao redirecionamento).
// Evita repetir o mesmo try/catch em dezenas de actions.
public abstract class BaseController : Controller
{
    protected IActionResult Tentar(Action acao, string mensagemSucesso, string actionDestino, string? controllerDestino = null, object? rotas = null)
    {
        try
        {
            acao();
            TempData["Mensagem"] = mensagemSucesso;
        }
        catch (RegraDeNegocioException ex)
        {
            TempData["Erro"] = ex.Message;   // mensagem da regra violada
        }
        return RedirectToAction(actionDestino, controllerDestino, rotas);
    }
}
