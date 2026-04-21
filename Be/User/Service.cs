namespace Be.User;

public interface IService
{
    IEnumerable<User> GetUsers(CancellationToken cancellationToken);
}

public class Service(IRepository repository) : IService
{
    public IEnumerable<User> GetUsers(CancellationToken cancellationToken)
        => repository.GetUsers(cancellationToken);
}
