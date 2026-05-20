using Microsoft.AspNetCore.Mvc;
using SecureFileCloud.API.Application.Dtos;
using SecureFileCloud.API.Application.Interfaces;
using SecureFileCloud.API.Presentation.Filters;

namespace SecureFileCloud.API.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
[ValidadorDeChave]
public class ArquivoController : ControllerBase
{
    private readonly IArquivoService _arquivoService;
    private readonly ILogger<ArquivoController> _logger;

    public ArquivoController(IArquivoService arquivoService, ILogger<ArquivoController> logger)
    {
        _arquivoService = arquivoService;
        _logger = logger;
    }

    [HttpPost("enviar")]
    public async Task<IActionResult> Upload(IFormFile arquivo, CancellationToken cancellationToken = default)
    {
        if (arquivo is null || arquivo.Length == 0)
            return BadRequest("Nenhum arquivo foi enviado.");

        _logger.LogInformation("Iniciando upload do arquivo {NomeArquivo}", arquivo.FileName);

        var inputUpload = new UploadArquivoInput(
            arquivo.FileName,
            arquivo.ContentType,
            arquivo.Length,
            arquivo.OpenReadStream
        );

        var response = await _arquivoService.UploadAsync(inputUpload, cancellationToken);

        return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
    }

    [HttpPost("enviar-multiplos")]
    public async Task<IActionResult> UploadMultiple(IReadOnlyCollection<IFormFile> arquivos, CancellationToken cancellationToken = default)
    {
        if (arquivos is null || arquivos.Count == 0)
            return BadRequest("Nenhum arquivo foi enviado.");

        _logger.LogInformation("Iniciando upload múltiplo. QuantidadeArquivos={QuantidadeArquivos}", arquivos.Count);

        var uploadInputs = arquivos
            .Select(u => new UploadArquivoInput(
                u.FileName,
                u.ContentType,
                u.Length,
                u.OpenReadStream
            ))
            .ToList();

        var response = await _arquivoService.UploadMultipleAsync(uploadInputs, cancellationToken);

        return Ok(response);
    }

    [HttpGet("baixar/{id:guid}")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando download do arquivo {ArquivoId}", id);

        var response = await _arquivoService.DownloadAsync(id, cancellationToken);
        if (response is null)
        {
            _logger.LogWarning("Falha no download. Arquivo {ArquivoId} não encontrado.", id);
            return NotFound("Arquivo não encontrado.");
        }

        return File(response.Conteudo, response.ContentType, response.NomeOriginal);
    }

    [HttpGet("baixar-multiplos")]
    public async Task<IActionResult> DownloadMultiple([FromQuery] IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids == null || ids.Count == 0)
            return BadRequest("Informe ao menos um ID.");

        _logger.LogInformation("Iniciando download múltiplo. QuantidadeIds={QuantidadeIds}", ids.Count);

        var response = await _arquivoService.DownloadMultipleAsync(ids, cancellationToken);
        if (response is null)
        {
            _logger.LogWarning("Falha no download múltiplo. Nenhum arquivo encontrado. QuantidadeIds={QuantidadeIds}", ids.Count);
            return NotFound("Nenhum arquivo encontrado.");
        }

        return File(response.Conteudo, response.ContentType, response.NomeArquivo);
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken = default)
    {
        var arquivos = await _arquivoService.ListarAsync(cancellationToken);
        return Ok(arquivos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken = default)
    {
        var arquivo = await _arquivoService.ObterMetadadosAsync(id, cancellationToken);

        if (arquivo is null)
            return NotFound("Arquivo não encontrado.");

        return Ok(arquivo);
    }
}
