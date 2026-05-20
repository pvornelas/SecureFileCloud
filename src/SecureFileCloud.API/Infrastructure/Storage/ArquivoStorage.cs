using SecureFileCloud.API.Application.Interfaces;

namespace SecureFileCloud.API.Infrastructure.Storage;

public class ArquivoStorage : IArquivoStorage
{
    private readonly string _basePath;
    private readonly ILogger<ArquivoStorage> _logger;

    public ArquivoStorage(IWebHostEnvironment environment, IConfiguration configuration, ILogger<ArquivoStorage> logger)
    {
        _logger = logger;

        var storagePath = configuration["Storage:BasePath"] ?? "AppData/FileStorage";
        _basePath = Path.Combine(environment.ContentRootPath, storagePath);

        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
            _logger.LogInformation("Storage de arquivos criado em: {BasePath}", _basePath);
        }
    }

    public async Task<ArquivoArmazenado> SalvarAsync(ArquivoParaArmazenar arquivo, CancellationToken cancellationToken = default)
    {
        var nomeOriginal = Path.GetFileName(arquivo.NomeOriginal);

        var extensao = Path.GetExtension(nomeOriginal).ToLowerInvariant();

        var nomeArmazenado = $"{Guid.NewGuid():N}{extensao}";

        var caminhoCompleto = Path.Combine(_basePath, nomeArmazenado);

        var options = new FileStreamOptions
        {
            Mode = FileMode.CreateNew,
            Access = FileAccess.Write,
            Share = FileShare.None,
            Options = FileOptions.Asynchronous
        };

        await using (var stream = new FileStream(caminhoCompleto, options))
        {
            await arquivo.Conteudo.CopyToAsync(stream, cancellationToken);
        }

        _logger.LogInformation("Arquivo {NomeOriginal} armazenado.", nomeOriginal);

        return new ArquivoArmazenado(
            nomeOriginal,
            nomeArmazenado,
            arquivo.ContentType,
            arquivo.TamanhoEmBytes
        );
    }

    public async Task<byte[]?> LerBytesAsync(string nomeArmazenado, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var caminhoCompleto = ObterCaminhoCompleto(nomeArmazenado);

        if (!File.Exists(caminhoCompleto))
        {
            _logger.LogWarning("Arquivo armazenado não encontrado: {NomeArmazenado}.", nomeArmazenado);
            return null;
        }

        return await File.ReadAllBytesAsync(caminhoCompleto, cancellationToken);
    }

    public bool Excluir(string nomeArmazenado)
    {
        var caminhoCompleto = ObterCaminhoCompleto(nomeArmazenado);

        if (!File.Exists(caminhoCompleto))
        {
            _logger.LogWarning("Tentativa de exclusão de arquivo inexistente: {NomeArmazenado}.", nomeArmazenado);
            return false;
        }

        File.Delete(caminhoCompleto);

        _logger.LogInformation("Arquivo {NomeArmazenado} excluído do storage.", nomeArmazenado);

        return true;
    }

    public bool Existe(string nomeArmazenado)
    {
        var caminhoCompleto = ObterCaminhoCompleto(nomeArmazenado);

        return File.Exists(caminhoCompleto);
    }

    private string ObterCaminhoCompleto(string nomeArmazenado)
    {
        var nomeSeguro = Path.GetFileName(nomeArmazenado);

        return Path.Combine(_basePath, nomeSeguro);
    }
}