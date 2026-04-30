using Beetles.Domain.Entities;

namespace Beetles.Application.Common.Interfaces;

public interface IBitemporalRepository
{
    IQueryable<T> QueryAll<T>() where T : class, IBitemporalEntity;

    Task<T> InsertAsync<T>(T entity, CancellationToken cancellationToken) where T : class, IBitemporalEntity;

    Task CommitChangesAsync(CancellationToken cancellationToken);
}
