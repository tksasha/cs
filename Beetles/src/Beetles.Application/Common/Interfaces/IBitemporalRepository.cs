using Beetles.Domain.Entities;

namespace Beetles.Application.Common.Interfaces;

public interface IBitemporalRepository
{
    IQueryable<T> QueryAll<T>() where T : BitemporalEntity;

    Task InsertAsync<T>(T entity, CancellationToken cancellationToken) where T : BitemporalEntity;

    Task UpdateAsync<T>(T entity, CancellationToken cancellationToken) where T : BitemporalEntity;

    Task<T> GetAsync<T>(int id, CancellationToken cancellationToken)
        where T : BitemporalEntity;

    Task<T> GetAsync<T>(int id, DateTimeOffset date, CancellationToken cancellationToken)
        where T : BitemporalEntity;

    Task CommitChangesAsync(CancellationToken cancellationToken);
}
