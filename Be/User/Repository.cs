using Microsoft.EntityFrameworkCore;

namespace Be.User;

public interface IRepository
{
    Task<IEnumerable<Model>> GetAllAsync(CancellationToken cancellationToken);

    Task<bool> CreateAsync(Model user, CancellationToken cancellationToken);

    Task<Model?> GetByIdAsync(int id, CancellationToken cancellationToken);
}

public class Repository(DatabaseContext databaseContext) : IRepository
{
    public async Task<IEnumerable<Model>> GetAllAsync(CancellationToken cancellationToken)
        => await databaseContext.Users.ToListAsync(cancellationToken);

    public async Task<bool> CreateAsync(Model user, CancellationToken cancellationToken)
    {
        databaseContext.Users.Add(user);

        return await databaseContext.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<Model?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => databaseContext.Users.AsNoTracking().FirstOrDefault(u => u.Id == id);
}
