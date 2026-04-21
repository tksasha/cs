namespace Be.Users;

public interface IService
{
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken);

    Task CreateAsync(CreateRequest request, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task UpdateAsync(int id, UpdateRequest request, CancellationToken cancellationToken);
}
