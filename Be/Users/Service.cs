namespace Be.Users;

public interface IService
{
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken);

    Task<bool> CreateAsync(CreateRequest request, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(int id, UpdateRequest request, CancellationToken cancellationToken);
}

public class Service(IRepository repository) : IService
{
    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken)
        => repository.GetAllAsync(cancellationToken);

    public async Task<bool> CreateAsync(CreateRequest request, CancellationToken cancellationToken)
    {
        var user = new User { Name = request.Name };

        return await repository.CreateAsync(user, cancellationToken);
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(id, cancellationToken);

    public async Task<bool> UpdateAsync(int id, UpdateRequest request, CancellationToken cancellationToken)
    {
        User? user = await repository.GetByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return false; // use Result or Expected to return an error
        }

        user.Name = request.Name;

        return await repository.UpdateAsync(cancellationToken);
    }
}
