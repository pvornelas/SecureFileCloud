using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SecureFileCloud.API.Presentation.Filters;

public class ValidadorDeChaveAttribute : ActionFilterAttribute
{
    private const string HeaderName = "X-Access-Token";

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var configuration = context.HttpContext
            .RequestServices
            .GetRequiredService<IConfiguration>();

        var tokenEsperado = configuration["Security:AccessToken"];

        var tokenInformado = context.HttpContext
            .Request
            .Headers[HeaderName]
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(tokenInformado))
        {
            context.Result = new UnauthorizedObjectResult("Token de acesso não informado.");
            return;
        }

        if (!string.Equals(tokenInformado, tokenEsperado, StringComparison.Ordinal))
        {
            context.Result = new UnauthorizedObjectResult("Token de acesso inválido.");
            return;
        }

        base.OnActionExecuting(context);
    }
}
