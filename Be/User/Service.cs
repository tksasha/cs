namespace Be.User;

public interface IService
{
    Task<IEnumerable<Model>> GetUsersAsync(CancellationToken cancellationToken);
}

public class Service(IRepository repository) : IService
{
    public Task<IEnumerable<Model>> GetUsersAsync(CancellationToken cancellationToken)
        => repository.GetUsersAsync(cancellationToken);
}
