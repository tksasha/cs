using Microsoft.EntityFrameworkCore;

namespace Be.User;

public interface IRepository
{
    Task<IEnumerable<Model>> GetUsersAsync(CancellationToken cancellationToken);

    Task<bool> CreateUserAsync(Model user, CancellationToken cancellationToken);
}

public class Repository(DatabaseContext databaseContext) : IRepository
{
    public async Task<IEnumerable<Model>> GetUsersAsync(CancellationToken cancellationToken)
        => await databaseContext.Users.ToListAsync(cancellationToken);

    public async Task<bool> CreateUserAsync(Model user, CancellationToken cancellationToken)
    {
        databaseContext.Users.Add(user);

        return await databaseContext.SaveChangesAsync(cancellationToken) > 0;
    }
}
