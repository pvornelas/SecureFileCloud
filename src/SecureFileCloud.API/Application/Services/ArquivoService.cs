using SecureFileCloud.API.Application.Dtos;
using SecureFileCloud.API.Application.Interfaces;
using SecureFileCloud.API.Domain.Entities;
using System.IO.Compression;

namespace SecureFileCloud.API.Application.Services;

public class ArquivoService : IArquivoService
{
    private readonly IArquivoRepository _arquivoRepository;
    private readonly IArquivoStorage _arquivoStorage;
    public ArquivoService(IArquivoRepository arquivoRepository, IArquivoStorage arquivoStorage)
    {
        _arquivoRepository = arquivoRepository;
        _arquivoStorage = arquivoStorage;
    }

    #region UPLOAD
    public async Task<UploadArquivoResponse> UploadAsync(UploadArquivoInput uploadInput, CancellationToken cancellationToken = default)
    {
        return await ProcessarUploadAsync(uploadInput, cancellationToken);
    }

    public async Task<IReadOnlyCollection<UploadArquivoResponse>> UploadMultipleAsync(IReadOnlyCollection<UploadArquivoInput> arquivos, CancellationToken cancellationToken = default)
    {
        var responses = new List<UploadArquivoResponse>();

        foreach (var arquivo in arquivos)
        {
            var response = await ProcessarUploadAsync(arquivo, cancellationToken);
            responses.Add(response);
        }

        return responses;
    }

    private async Task<UploadArquivoResponse> ProcessarUploadAsync(UploadArquivoInput uploadInput, CancellationToken cancellationToken = default)
    {
        await using var stream = uploadInput.AbrirStream();

        var arquivoParaArmazenar = new ArquivoParaArmazenar(
            uploadInput.NomeOriginal,
            uploadInput.ContentType,
            uploadInput.TamanhoEmBytes,
            stream
        );

        var arquivoArmazenado = await _arquivoStorage.SalvarAsync(arquivoParaArmazenar, cancellationToken);

        var metadata = Arquivo.Create(
            arquivoArmazenado.NomeOriginal,
            arquivoArmazenado.NomeArmazenado,
            arquivoArmazenado.ContentType,
            arquivoArmazenado.TamanhoEmBytes
        );

        try
        {
            await _arquivoRepository.AdicionarAsync(metadata, cancellationToken);
        }
        catch
        {
            _arquivoStorage.Excluir(arquivoArmazenado.NomeArmazenado);
            throw;
        }

        return new UploadArquivoResponse(
           metadata.Id,
           metadata.NomeOriginal,
           metadata.ContentType,
           metadata.TamanhoEmBytes,
           metadata.DataUpload
        );
    }
    #endregion

    #region DOWNLOAD
    public async Task<DownloadArquivoResponse?> DownloadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var metadata = await _arquivoRepository.ObterPorIdAsync(id, cancellationToken);
        if (metadata is null)
            return null;

        var conteudo = await _arquivoStorage.LerBytesAsync(metadata.NomeArmazenado, cancellationToken);

        if (conteudo is null)
            return null;

        return new DownloadArquivoResponse(
            metadata.NomeOriginal,
            metadata.ContentType,
            conteudo
        );
    }

    public async Task<DownloadMultiplosResponse?> DownloadMultipleAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        var arquivos = new List<Arquivo>();

        foreach (var id in ids)
        {
            var metadata = await _arquivoRepository.ObterPorIdAsync(id, cancellationToken);

            if (metadata is not null)
                arquivos.Add(metadata);
        }

        if (arquivos.Count == 0)
            return null;

        await using var zipStream = new MemoryStream();

        var entradasCriadas = 0;
        var nomesUsados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var metadata in arquivos)
            {
                var conteudo = await _arquivoStorage.LerBytesAsync(metadata.NomeArmazenado, cancellationToken);

                if (conteudo is null) continue;

                var nomeEntradaZip = GerarNomeEntradaZip(metadata.NomeOriginal, nomesUsados);

                var entradaZip = zipArchive.CreateEntry(nomeEntradaZip, CompressionLevel.Fastest);

                await using var entradaStream = entradaZip.Open();

                await entradaStream.WriteAsync(conteudo, cancellationToken);

                entradasCriadas++;
            }
        }

        if (entradasCriadas == 0) return null;

        var nomeZip = $"securefile-download-{DateTime.UtcNow:yyyyMMddHHmmss}.zip";

        var conteudoZip = zipStream.ToArray();

        return new DownloadMultiplosResponse(nomeZip, "application/zip", conteudoZip);
    }

    private static string GerarNomeEntradaZip(string nomeOriginal, HashSet<string> nomesUsados)
    {
        var nomeSeguro = Path.GetFileName(nomeOriginal);

        if (nomesUsados.Add(nomeSeguro))
            return nomeSeguro;

        var nomeSemExtensao = Path.GetFileNameWithoutExtension(nomeSeguro);
        var extensao = Path.GetExtension(nomeSeguro);

        var contador = 2;

        while (true)
        {
            var candidato = $"{nomeSemExtensao}-{contador}{extensao}";

            if (nomesUsados.Add(candidato))
                return candidato;

            contador++;
        }
    }
    #endregion

    #region LISTAR/OBTER
    public async Task<IReadOnlyCollection<ArquivoMetadadosResponse>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var arquivos = await _arquivoRepository.ListarAsync(cancellationToken);

        return arquivos.Select(MapearParaMetadadosResponse).ToList();
    }

    public async Task<ArquivoMetadadosResponse?> ObterMetadadosAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var arquivo = await _arquivoRepository.ObterPorIdAsync(id, cancellationToken);

        if (arquivo is null)
            return null;

        return MapearParaMetadadosResponse(arquivo);
    }

    private static ArquivoMetadadosResponse MapearParaMetadadosResponse(Arquivo arquivo)
    {
        return new ArquivoMetadadosResponse(
            arquivo.Id,
            arquivo.NomeOriginal,
            arquivo.ContentType,
            arquivo.TamanhoEmBytes,
            arquivo.DataUpload
        );
    }
    #endregion
}