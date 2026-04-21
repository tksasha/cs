using Microsoft.EntityFrameworkCore;

namespace Be.Users;

public class Repository(DatabaseContext databaseContext) : IRepository
{
    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken)
        => await databaseContext.Users.AsNoTracking().ToListAsync(cancellationToken);

    public async Task CreateAsync(User user, CancellationToken cancellationToken)
    {
        databaseContext.Users.Add(user);

        await databaseContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => databaseContext.Users.FirstOrDefault(u => u.Id == id);

    public async Task UpdateAsync(CancellationToken cancellationToken)
        => await databaseContext.SaveChangesAsync(cancellationToken);
}
