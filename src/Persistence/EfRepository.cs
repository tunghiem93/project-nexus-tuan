using Microsoft.EntityFrameworkCore;
using Nexus.Abstractions.Persistence;

namespace Nexus.User.Persistence;

public sealed class EfRepository<T> : IRepository<T>
    where T : class
{
    private readonly DbSet<T> _entities;

    public EfRepository(DbContext dbContext)
    {
        _entities = dbContext.Set<T>();
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _entities.AddAsync(entity, cancellationToken);
    }

    public IQueryable<T> Query() => _entities.AsQueryable();

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _entities.FindAsync(new object[] { id }, cancellationToken).AsTask();
    }

    public void Remove(T entity) => _entities.Remove(entity);

    public void Update(T entity) => _entities.Update(entity);
}
