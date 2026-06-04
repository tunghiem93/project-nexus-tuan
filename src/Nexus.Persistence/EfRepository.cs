using Microsoft.EntityFrameworkCore;
using Nexus.Abstractions.Persistence;

namespace Nexus.Persistence;

public class EfRepository<T>(NexusDbContext context) : IRepository<T>
    where T : class
{
    protected readonly DbSet<T> Set = context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await Set.FindAsync([id], cancellationToken);

    public virtual IQueryable<T> Query() => Set.AsQueryable();

    public virtual Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => Set.AddAsync(entity, cancellationToken).AsTask();

    public virtual void Update(T entity) => Set.Update(entity);

    public virtual void Remove(T entity) => Set.Remove(entity);
}
