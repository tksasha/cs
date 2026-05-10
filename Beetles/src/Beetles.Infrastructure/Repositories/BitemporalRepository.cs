using Beetles.Application.Common.Interfaces;
using Beetles.Application.Exceptions;
using Beetles.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Beetles.Infrastructure.Repositories;

internal sealed class BitemporalRepository(
    DatabaseContext context,
    TimeProvider timeProvider) : IBitemporalRepository
{
    public IQueryable<T> QueryAll<T>()
        where T : BitemporalEntity
    {
        var entities = context.Set<T>().AsNoTracking();

        return entities;
    }

    public async Task InsertAsync<T>(T entity, CancellationToken cancellationToken)
        where T : BitemporalEntity
    {
        entity.SystemStart = timeProvider.GetUtcNow();

        await context.Set<T>().AddAsync(entity, cancellationToken);
    }

    public async Task<T> GetAsync<T>(int id, CancellationToken cancellationToken)
        where T : BitemporalEntity
        => await context.Set<T>().FirstOrDefaultAsync(e =>
            e.Id == id
            && e.BusinessEnd == DateTimeOffset.MaxValue
            && e.SystemEnd == DateTimeOffset.MaxValue, cancellationToken)
        ?? throw new NotFoundException();


    public async Task<T> GetAsync<T>(int id, DateTimeOffset date, CancellationToken cancellationToken)
        where T : BitemporalEntity
        => await context.Set<T>().FirstOrDefaultAsync(e =>
            e.Id == id
            && e.BusinessStart <= date
            && e.BusinessEnd > date
            && e.SystemEnd == DateTimeOffset.MaxValue, cancellationToken)
        ?? throw new NotFoundException();

    private async Task Supersede<T>(T entity)
        where T : BitemporalEntity
    {
        entity.SystemEnd = timeProvider.GetUtcNow();

        context.Set<T>().Update(entity);
    }

    private async Task CloseAsync<T>(T entity, DateTimeOffset businessEnd, CancellationToken cancellationToken)
        where T : BitemporalEntity
    {
        entity.BusinessEnd = businessEnd;
        entity.SystemStart = timeProvider.GetUtcNow();
        entity.SystemEnd = DateTimeOffset.MaxValue;

        await context.Set<T>().AddAsync(entity, cancellationToken);
    }

    private async Task AppendAsync<T>(T entity, CancellationToken cancellationToken)
        where T : BitemporalEntity
    {
        var current = await GetAsync<T>(entity.Id, cancellationToken);

        entity.BusinessEnd = current.BusinessStart > entity.BusinessStart
            ? current.BusinessStart
            : DateTimeOffset.MaxValue;

        await InsertAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync<T>(T entity, CancellationToken cancellationToken)
        where T : BitemporalEntity
    {
        var actual = await GetAsync<T>(entity.Id, entity.BusinessStart, cancellationToken);

        await Supersede(actual);

        await CloseAsync((T)actual.Clone(), businessEnd: entity.BusinessStart, cancellationToken);

        await AppendAsync(entity, cancellationToken);
    }

    public Task CommitChangesAsync(CancellationToken cancellationToken)
        => context.SaveChangesAsync(cancellationToken);
}
