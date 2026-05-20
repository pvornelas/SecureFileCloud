using SecureFileCloud.API.Application.Dtos;

namespace SecureFileCloud.API.Application.Interfaces;

public interface IArquivoService
{
    Task<UploadArquivoResponse> UploadAsync(UploadArquivoInput arquivo, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UploadArquivoResponse>> UploadMultipleAsync(IReadOnlyCollection<UploadArquivoInput> arquivos, CancellationToken cancellationToken = default);
    Task<DownloadArquivoResponse?> DownloadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DownloadMultiplosResponse?> DownloadMultipleAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ArquivoMetadadosResponse>> ListarAsync(CancellationToken cancellationToken = default);
    Task<ArquivoMetadadosResponse?> ObterMetadadosAsync(Guid id, CancellationToken cancellationToken = default);
}
