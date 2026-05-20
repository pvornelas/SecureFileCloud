using Microsoft.EntityFrameworkCore;
using SecureFileCloud.API.Domain.Entities;

namespace SecureFileCloud.API.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Arquivo> Arquivos { get; set; }
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }
}
