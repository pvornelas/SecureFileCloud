using System.Diagnostics;

namespace SecureFileCloud.API.Presentation.Middleware;

public class AuditoriaMiddleware
{
    private const long BytesPorMegabyte = 1024 * 1024;
    private const long TamanhoLimiteRequisicao = 5 * BytesPorMegabyte;
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditoriaMiddleware> _logger;

    public AuditoriaMiddleware(RequestDelegate next, ILogger<AuditoriaMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var metodoHttp = context.Request.Method;
        var caminhoRecurso = context.Request.Path;
        var contentLength = context.Request.ContentLength;

        if (contentLength > TamanhoLimiteRequisicao)
        {
            var tamanhoRequisicaoMb = ConverterBytesParaMb(contentLength.Value);
            var tamanhoLimiteMb = ConverterBytesParaMb(TamanhoLimiteRequisicao);

             _logger.LogWarning(
                "Requisição bloqueada por exceder o limite de tamanho. Metodo={MetodoHttp}, Caminho={CaminhoRecurso}, Tamanho={TamanhoRequisicaoMb:F2} MB, Limite={TamanhoLimiteMb:F2} MB", 
                metodoHttp,  
                caminhoRecurso,
                tamanhoRequisicaoMb,
                tamanhoLimiteMb
            );

            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            await context.Response.WriteAsync($"O tamanho da requisição excede o limite permitido de {tamanhoLimiteMb} MB.");

            return;
        }

        var cronometro = Stopwatch.StartNew();

        await _next(context);

        _logger.LogInformation("Requisição processada. Método={MetodoHttp}, Caminho={CaminhoRecurso}, StatusCode={Status}, Tempo={TempoProcessamento} ms",
            metodoHttp, caminhoRecurso, context.Response.StatusCode, cronometro.ElapsedMilliseconds);
    }

    private static double ConverterBytesParaMb(long bytes)
    {
        return bytes / (double)BytesPorMegabyte;
    }
}
