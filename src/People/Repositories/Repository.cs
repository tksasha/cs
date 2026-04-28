using Microsoft.EntityFrameworkCore;

using People.Entities;

namespace People.Repositories;

internal class Repository(DatabaseContext context) : IRepository
{
    public IQueryable<T> GetAll<T>() where T : class, IBitemporalEntity
        => context.Set<T>().AsNoTracking();
}
