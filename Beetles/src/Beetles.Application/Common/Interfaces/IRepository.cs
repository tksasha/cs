using Beetles.Domain.Entities;

namespace Beetles.Application.Common.Interfaces;

public interface IRepository
{
    IQueryable<T> QueryAll<T>() where T : Entity;

    Task<T> InsertAsync<T>(T entity, CancellationToken cancellationToken) where T : Entity;

    Task<T> GetByIdAsync<T>(int id, CancellationToken cancellationToken) where T : Entity;

    void Update<T>(T entity) where T : Entity;

    Task CommitChangesAsync(CancellationToken cancellationToken);
}
