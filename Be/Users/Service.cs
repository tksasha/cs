namespace Be.Users;

public class Service(IRepository repository) : IService
{
    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken)
        => repository.GetAllAsync(cancellationToken);

    public async Task CreateAsync(CreateRequest request, CancellationToken cancellationToken)
    {
        var user = new User { Name = request.Name };

        await repository.CreateAsync(user, cancellationToken);
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(id, cancellationToken);

    public async Task UpdateAsync(int id, UpdateRequest request, CancellationToken cancellationToken)
    {
        User? user = await repository.GetByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return; // use Result or Expected to return an error
        }

        user.Name = request.Name;

        await repository.UpdateAsync(cancellationToken);
    }
}
