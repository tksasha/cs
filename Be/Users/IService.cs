namespace Be.Users;

public interface IService
{
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken);

    Task<bool> CreateAsync(CreateRequest request, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task UpdateAsync(Guid id, UpdateRequest request, CancellationToken cancellationToken);
}
