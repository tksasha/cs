namespace Be.Users;

public interface IRepository
{
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken);

    Task CreateAsync(User user, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task UpdateAsync(CancellationToken cancellationToken);
}
