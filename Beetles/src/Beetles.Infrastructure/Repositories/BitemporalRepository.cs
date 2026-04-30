using Beetles.Application.Common.Interfaces;
using Beetles.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beetles.Infrastructure.Repositories;

internal sealed class BitemporalRepository(DatabaseContext context) : IBitemporalRepository
{
    public IQueryable<T> QueryAll<T>() where T : class, IBitemporalEntity
        => context.Set<T>().AsNoTracking();

    public async Task<T> InsertAsync<T>(T entity, CancellationToken cancellationToken)
        where T : class, IBitemporalEntity
    {
        await context.Set<T>().AddAsync(entity, cancellationToken);

        return entity;
    }

    public Task CommitChangesAsync(CancellationToken cancellationToken)
        => context.SaveChangesAsync(cancellationToken);
}
