using Beetles.Application.Common.Interfaces;
using Beetles.Application.Exceptions;
using Beetles.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beetles.Infrastructure.Repositories;

internal sealed class BitemporalRepository(DatabaseContext context) : IBitemporalRepository
{
    public IQueryable<T> QueryAll<T>() where T : class, IBitemporalEntity
        => context.Set<T>().AsNoTracking().Where(b => b.RecordedTo == DateTimeOffset.MaxValue);

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

    private async Task ValidateValidFromIsUnique<T>(T entity, CancellationToken cancellationToken)
        where T : class, IBitemporalEntity
    {
        bool any = await context.Set<T>()
            .AnyAsync(e => e.Id == entity.Id
                && e.ValidFrom == entity.ValidFrom, cancellationToken);

        if (any) throw new ConflictException();
    }


    public async Task UpdateAsync<T>(T currentVersion, T newVersion, CancellationToken cancellationToken)
        where T : class, IBitemporalEntity
    {
        await ValidateValidFromIsUnique(newVersion, cancellationToken);

        currentVersion.RecordedTo = DateTimeOffset.UtcNow;

        context.Set<T>().Update(currentVersion);

        newVersion.RecordedFrom = DateTimeOffset.UtcNow;
        newVersion.RecordedTo = DateTimeOffset.MaxValue;

        await context.Set<T>().AddAsync(newVersion, cancellationToken);
    }

    public async Task DeleteAsync<T>(int id, CancellationToken cancellationToken)
        where T : class, IBitemporalEntity
    {
        var entity = await GetByIdAsync<T>(id, cancellationToken);

        entity.RecordedTo = DateTimeOffset.UtcNow;

        context.Set<T>().Update(entity);
    }

    public Task CommitChangesAsync(CancellationToken cancellationToken)
        => context.SaveChangesAsync(cancellationToken);
}
