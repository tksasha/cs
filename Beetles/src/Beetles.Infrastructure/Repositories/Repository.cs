using Beetles.Application.Common.Interfaces;
using Beetles.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Beetles.Infrastructure.Repositories;

internal sealed class Repository(ApplicationDbContext context) : IRepository
{
    public IQueryable<T> QueryAll<T>() where T : class, IEntity
        => context.Set<T>().AsNoTracking();

    public async Task<T> GetByIdAsync<T>(int id, CancellationToken cancellationToken) where T : class, IEntity
    {
        var entity = await context.Set<T>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity is not null) return entity;

        throw new Exception("Record not found"); // TODO: use a separate class
    }

    public async Task<T> InsertAsync<T>(T entity, CancellationToken cancellationToken) where T : class, IEntity
    {
        await context.Set<T>().AddAsync(entity, cancellationToken);

        return entity;
    }

    public Task CommitChangesAsync(CancellationToken cancellationToken)
        => context.SaveChangesAsync(cancellationToken);
}
