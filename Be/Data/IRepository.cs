namespace Be.Data;

public interface IRepository<T> where T : IEntity
{
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);

    Task InsertAsync(T entity, CancellationToken cancellationToken);

    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task UpdateAsync(CancellationToken cancellationToken);

    Task<T?> GetCurrentAsync(Guid id, CancellationToken cancellationToken);
}
