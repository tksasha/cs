using Beetles.Application.Common.Interfaces;
using Beetles.Application.Exceptions;
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

    public async Task<T> GetByIdAsync<T>(int id, CancellationToken cancellationToken)
        where T : class, IBitemporalEntity
    {
        var entity = await context.Set<T>()
            .FirstOrDefaultAsync(e => e.Id == id
                && e.ValidTo == DateTimeOffset.MaxValue
                && e.RecordedTo == DateTimeOffset.MaxValue, cancellationToken);

        if (entity is not null) return entity;

        throw new NotFoundException();
    }

    public async Task UpdateAsync<T>(T entity, CancellationToken cancellationToken)
        where T : class, IBitemporalEntity
    {
        var current = await GetByIdAsync<T>(entity.Id, cancellationToken);

        current.RecordedTo = DateTimeOffset.MaxValue;

        context.Set<T>().Update(current);

        entity.RecordedFrom = DateTimeOffset.UtcNow;

        context.Set<T>().Update(entity);
    }

    public Task CommitChangesAsync(CancellationToken cancellationToken)
        => context.SaveChangesAsync(cancellationToken);
}
