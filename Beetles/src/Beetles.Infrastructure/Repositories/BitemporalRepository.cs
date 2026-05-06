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

    public async Task<T> GetAsync<T>(int id, DateTimeOffset date, CancellationToken cancellationToken)
        where T : BitemporalEntity
    => await context.Set<T>()
        .FirstOrDefaultAsync(e => e.Id == id
            && e.BusinessStart <= date
            && e.BusinessEnd > date
            && e.SystemEnd == DateTimeOffset.MaxValue, cancellationToken)
        ?? throw new NotFoundException();

    // private static void Log<T>(string name, T entity) where T : BitemporalEntity
    // {
    //     Console.WriteLine($@"
    //         [DEBUG] {name}, {entity.Id}
    //         {entity.BusinessStart.Date}, {entity.BusinessEnd?.Date},
    //         {entity.SystemStart.Date}, {entity.SystemEnd.Date}");
    // }

    public async Task UpdateAsync<T>(T entity, CancellationToken cancellationToken)
        where T : BitemporalEntity
    {
        var actual = await GetAsync<T>(entity.Id, entity.BusinessStart, cancellationToken);

        actual.SystemEnd = timeProvider.GetUtcNow();

        context.Set<T>().Update(actual);

        // Log(nameof(actual), actual); // delme

        var closed = (T)actual.Clone();

        closed.BusinessEnd = entity.BusinessStart;
        closed.SystemStart = timeProvider.GetUtcNow();
        closed.SystemEnd = DateTimeOffset.MaxValue;

        await context.Set<T>().AddAsync(closed, cancellationToken);

        // Log(nameof(closed), closed); // delme

        entity.SystemStart = timeProvider.GetUtcNow();

        await context.Set<T>().AddAsync(entity, cancellationToken);

        // Log(nameof(entity), entity); // delme
    }

    public Task CommitChangesAsync(CancellationToken cancellationToken)
        => context.SaveChangesAsync(cancellationToken);
}
