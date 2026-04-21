namespace Be.User;

public interface IRepository
{
    IEnumerable<User> GetUsers(CancellationToken cancellationToken);
}

public class Repository : IRepository
{
    readonly List<User> _users = [new() { Name = "Bruce Wayne" }];

    public IEnumerable<User> GetUsers(CancellationToken _)
        => _users;
}
