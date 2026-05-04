using Beetles.Application.Common.Interfaces;
using Beetles.Application.Exceptions;
using Beetles.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beetles.Infrastructure.Repositories;

internal sealed class BitemporalRepository(DatabaseContext context) : IBitemporalRepository
{
    public IQueryable<T> QueryAll<T>(DateTimeOffset? valid = null, DateTimeOffset? recorded = null)
        where T : BitemporalEntity
    {
        var entities = context.Set<T>().AsNoTracking();

        entities = recorded is not null
            ? entities.Where(e => e.RecordedFrom <= recorded && e.RecordedTo > recorded)
            : entities.Where(e => e.RecordedTo == DateTimeOffset.MaxValue);

        return entities;
    }

    public async Task<T> InsertAsync<T>(T entity, CancellationToken cancellationToken)
        where T : BitemporalEntity
    {
        await context.Set<T>().AddAsync(entity, cancellationToken);

        return entity;
    }

    public async Task<T> GetByIdAsync<T>(int id, CancellationToken cancellationToken)
        where T : BitemporalEntity
    {
        var entity = await context.Set<T>()
            .FirstOrDefaultAsync(e => e.Id == id
                && e.ValidTo == DateTimeOffset.MaxValue
                && e.RecordedTo == DateTimeOffset.MaxValue, cancellationToken);

        if (entity is not null) return entity;

        throw new NotFoundException();
    }

    private async Task ValidateValidFromIsUnique<T>(T entity, CancellationToken cancellationToken)
        where T : BitemporalEntity
    {
        bool any = await context.Set<T>()
            .AnyAsync(e => e.Id == entity.Id
                && e.ValidFrom == entity.ValidFrom, cancellationToken);

        if (any) throw new ConflictException();
    }


    public async Task UpdateAsync<T>(T currentVersion, T newVersion, CancellationToken cancellationToken)
        where T : BitemporalEntity
    {
        await ValidateValidFromIsUnique(newVersion, cancellationToken);

        currentVersion.RecordedTo = DateTimeOffset.UtcNow;

        context.Set<T>().Update(currentVersion);

        newVersion.RecordedFrom = DateTimeOffset.UtcNow;
        newVersion.RecordedTo = DateTimeOffset.MaxValue;

        await context.Set<T>().AddAsync(newVersion, cancellationToken);
    }

    public async Task DeleteAsync<T>(int id, CancellationToken cancellationToken)
        where T : BitemporalEntity
    {
        var entity = await GetByIdAsync<T>(id, cancellationToken);

        entity.RecordedTo = DateTimeOffset.UtcNow;

        context.Set<T>().Update(entity);
    }

    public Task CommitChangesAsync(CancellationToken cancellationToken)
        => context.SaveChangesAsync(cancellationToken);
}
