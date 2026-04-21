using Microsoft.EntityFrameworkCore;

namespace Be.User;

public interface IRepository
{
    Task<IEnumerable<Model>> GetUsersAsync(CancellationToken cancellationToken);
}

public class Repository(DatabaseContext databaseContext) : IRepository
{
    public async Task<IEnumerable<Model>> GetUsersAsync(CancellationToken cancellationToken)
        => await databaseContext.Users.ToListAsync(cancellationToken);
}
