using Microsoft.EntityFrameworkCore;

namespace Be.Data;

public class Repository<T>(DatabaseContext context) : IRepository<T> where T : class, IEntity
{
    private readonly DbSet<T> _set = context.Set<T>();

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken)
        => await _set.AsNoTracking().ToListAsync(cancellationToken);

    public async Task CreateAsync(T entity, CancellationToken cancellationToken)
    {
        _set.Add(entity);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => await _set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task UpdateAsync(CancellationToken cancellationToken)
        => await context.SaveChangesAsync(cancellationToken);
}
