using AgendaBella.Negocio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AgendaBella.Api.Seguranca;

// FILTRO DE EXCEÇÃO: em vez de escrever try/catch em todo endpoint, este filtro intercepta
// qualquer RegraDeNegocioException lançada pela camada Negocio e devolve:
//     400 Bad Request   { "erro": "mensagem da regra" }
// Registrado uma única vez no Program.cs (AddControllers).
public class FiltroRegraDeNegocio : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not RegraDeNegocioException ex) return;

        context.Result = new BadRequestObjectResult(new { erro = ex.Message });
        context.ExceptionHandled = true;
    }
}
