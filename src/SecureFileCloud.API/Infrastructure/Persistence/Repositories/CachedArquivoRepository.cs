using System.Collections.Concurrent;
using SecureFileCloud.API.Application.Interfaces;
using SecureFileCloud.API.Domain.Entities;

namespace SecureFileCloud.API.Infrastructure.Persistence.Repositories;

public class CachedArquivoRepository : IArquivoRepository
{
    private readonly IArquivoRepository _innerRepository;
    
    // Uso de ConcurrentDictionary por segurança em ambiente com múltiplas requisições.
    private static readonly ConcurrentDictionary<Guid, Arquivo> _arquivosEmMemoria = new();

    public CachedArquivoRepository(IArquivoRepository innerRepository)
    {
        _innerRepository = innerRepository;        
    }

    public async Task AdicionarAsync(Arquivo arquivo, CancellationToken cancellationToken = default)
    {
        await _innerRepository.AdicionarAsync(arquivo, cancellationToken);

        _arquivosEmMemoria[arquivo.Id] = arquivo;
    }

    public async Task<IReadOnlyCollection<Arquivo>> ListarAsync(CancellationToken cancellationToken = default)
    {
        if(_arquivosEmMemoria.IsEmpty)
        {
            var arquivosDoBanco = await _innerRepository.ListarAsync(cancellationToken);

            foreach(var arquivo in arquivosDoBanco)
            {
                _arquivosEmMemoria[arquivo.Id] = arquivo;
            }
        }

        return _arquivosEmMemoria.Values
            .OrderByDescending(a => a.DataUpload)
            .ToList();
    }

    public async Task<Arquivo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_arquivosEmMemoria.TryGetValue(id, out var arquivoEmMemoria))
        {
            return arquivoEmMemoria;
        }

        var arquivoDoBanco = await _innerRepository.ObterPorIdAsync(id, cancellationToken);

        if (arquivoDoBanco is not null)
            _arquivosEmMemoria[arquivoDoBanco.Id] = arquivoDoBanco;

        return arquivoDoBanco;
    }
}
