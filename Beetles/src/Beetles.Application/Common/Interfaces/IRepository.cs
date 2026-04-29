using Beetles.Domain.Entities;

namespace Beetles.Application.Common.Interfaces;

public interface IRepository
{
    IQueryable<T> QueryAll<T>() where T : class, IEntity;

    Task<T> GetByIdAsync<T>(int id, CancellationToken cancellationToken) where T : class, IEntity;
}
