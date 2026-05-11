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

    private void Supersede<T>(T entity)
        where T : BitemporalEntity
    {
        entity.SystemEnd = timeProvider.GetUtcNow();

        context.Set<T>().Update(entity);
    }

    private async Task CloseAsync<T>(T actual, DateTimeOffset businessStart, CancellationToken cancellationToken)
        where T : BitemporalEntity
    {
        if (actual.BusinessStart == businessStart) return;

        var entity = (T)actual.Clone();
        entity.BusinessEnd = businessStart;
        entity.SystemEnd = DateTimeOffset.MaxValue;
        entity.TransactionId = 0;

        await InsertAsync(entity, cancellationToken);
    }

    private Task AppendAsync<T>(T entity, CancellationToken cancellationToken) where T : BitemporalEntity
        => AppendAsync(entity, DateTimeOffset.MaxValue, cancellationToken);

    private async Task AppendAsync<T>(T entity, DateTimeOffset businessEnd, CancellationToken cancellationToken)
        where T : BitemporalEntity
    {
        var clone = (T)entity.Clone();

        clone.BusinessEnd = businessEnd;
        clone.SystemEnd = DateTimeOffset.MaxValue;
        clone.TransactionId = 0;

        await InsertAsync(clone, cancellationToken);
    }

    public async Task UpdateAsync<T>(T entity, CancellationToken cancellationToken)
        where T : BitemporalEntity
    {
        var actual = await GetAsync<T>(entity.Id, entity.BusinessStart, cancellationToken);

        Supersede(actual);

        await CloseAsync(actual, entity.BusinessStart, cancellationToken);

        await AppendAsync(entity, actual.BusinessEnd ?? DateTimeOffset.MaxValue, cancellationToken);
    }

    public async Task DeleteAsync<T>(int id, DateTimeOffset date, CancellationToken cancellationToken)
        where T : BitemporalEntity
    {
        await context.Set<T>().Where(e =>
            e.Id == id
            && e.BusinessStart <= date
            && e.BusinessEnd >= date
            && e.SystemEnd == DateTimeOffset.MaxValue
        ).ForEachAsync(Supersede, cancellationToken);

        var actual = await GetAsync<T>(id, date.AddDays(-1), cancellationToken);

        await AppendAsync(actual, cancellationToken);
    }

    public Task CommitChangesAsync(CancellationToken cancellationToken)
        => context.SaveChangesAsync(cancellationToken);
}
