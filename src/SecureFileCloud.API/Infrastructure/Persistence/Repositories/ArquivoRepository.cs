using Microsoft.EntityFrameworkCore;
using SecureFileCloud.API.Application.Interfaces;
using SecureFileCloud.API.Domain.Entities;

namespace SecureFileCloud.API.Infrastructure.Persistence.Repositories;

public class ArquivoRepository : IArquivoRepository
{
    private readonly AppDbContext _context;
    public ArquivoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(Arquivo arquivo, CancellationToken cancellationToken = default)
    {
        _context.Arquivos.Add(arquivo);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Arquivo>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Arquivos
            .AsNoTracking()
            .OrderByDescending(a => a.DataUpload)
            .ToListAsync(cancellationToken);
    }

    public async Task<Arquivo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Arquivos
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
}
