namespace Be.User;

public interface IService
{
    Task<IEnumerable<Model>> GetAllAsync(CancellationToken cancellationToken);

    Task<bool> CreateAsync(CreateRequest request, CancellationToken cancellationToken);

    Task<Model?> GetByIdAsync(int id, CancellationToken cancellationToken);
}

public class Service(IRepository repository) : IService
{
    public Task<IEnumerable<Model>> GetAllAsync(CancellationToken cancellationToken)
        => repository.GetAllAsync(cancellationToken);

    public async Task<bool> CreateAsync(CreateRequest request, CancellationToken cancellationToken)
    {
        var user = new Model { Name = request.Name };

        return await repository.CreateAsync(user, cancellationToken);
    }

    public async Task<Model?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(id, cancellationToken);
}
