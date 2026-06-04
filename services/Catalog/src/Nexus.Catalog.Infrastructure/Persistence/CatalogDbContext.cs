using Microsoft.EntityFrameworkCore;
using Nexus.Persistence;
using Nexus.Catalog.Domain.Entities;

namespace Nexus.Catalog.Infrastructure.Persistence;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : NexusDbContext(options)
{
    public DbSet<Category> Categorys => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}
