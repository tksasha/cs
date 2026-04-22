namespace Be.Data;

public interface IRepository<T> where T : IEntity
{
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);

    Task CreateAsync(T entity, CancellationToken cancellationToken);

    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task UpdateAsync(CancellationToken cancellationToken);
}
