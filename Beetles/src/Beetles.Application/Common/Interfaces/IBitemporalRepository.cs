using Beetles.Domain.Entities;

namespace Beetles.Application.Common.Interfaces;

public interface IBitemporalRepository
{
    IQueryable<T> QueryAll<T>() where T : class, IBitemporalEntity;

    Task<T> InsertAsync<T>(T entity, CancellationToken cancellationToken) where T : class, IBitemporalEntity;

    Task<T> GetByIdAsync<T>(int id, CancellationToken cancellationToken) where T : class, IBitemporalEntity;

    Task UpdateAsync<T>(T currentVersion, T newVersion, CancellationToken cancellationToken)
        where T : class, IBitemporalEntity;

    Task CommitChangesAsync(CancellationToken cancellationToken);
}
