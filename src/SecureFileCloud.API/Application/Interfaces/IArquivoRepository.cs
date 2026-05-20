using SecureFileCloud.API.Domain.Entities;

namespace SecureFileCloud.API.Application.Interfaces;

public interface IArquivoRepository
{
    Task AdicionarAsync(Arquivo arquivo, CancellationToken cancellationToken = default);
    Task<Arquivo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Arquivo>> ListarAsync(CancellationToken cancellationToken = default);
}
