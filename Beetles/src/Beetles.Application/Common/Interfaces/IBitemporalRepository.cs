using Beetles.Domain.Entities;

namespace Beetles.Application.Common.Interfaces;

public interface IBitemporalRepository
{
    IQueryable<T> QueryAll<T>(DateTimeOffset? valid = null, DateTimeOffset? recorded = null)
        where T : BitemporalEntity;

    Task InsertAsync<T>(T entity, CancellationToken cancellationToken) where T : BitemporalEntity;

    Task<T> GetByIdAsync<T>(int id, CancellationToken cancellationToken) where T : BitemporalEntity;

    Task UpdateAsync<T>(T currentVersion, T newVersion, CancellationToken cancellationToken)
        where T : BitemporalEntity;

    Task CorrectAsync<T>(T currentVersion, T newVersion, CancellationToken cancellationToken)
        where T : BitemporalEntity;

    Task DeleteAsync<T>(int id, CancellationToken cancellationToken) where T : BitemporalEntity;

    Task CommitChangesAsync(CancellationToken cancellationToken);
}
