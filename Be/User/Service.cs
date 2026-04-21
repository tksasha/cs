namespace Be.User;

public interface IService
{
    Task<IEnumerable<Model>> GetUsersAsync(CancellationToken cancellationToken);

    Task<bool> CreateUserAsync(CreateRequest request, CancellationToken cancellationToken);
}

public class Service(IRepository repository) : IService
{
    public Task<IEnumerable<Model>> GetUsersAsync(CancellationToken cancellationToken)
        => repository.GetUsersAsync(cancellationToken);

    public async Task<bool> CreateUserAsync(CreateRequest request, CancellationToken cancellationToken)
    {
        var user = new Model { Name = request.Name };

        return await repository.CreateUserAsync(user, cancellationToken);
    }
}
