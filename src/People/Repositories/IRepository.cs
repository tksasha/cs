using People.Entities;

namespace People.Repositories;

public interface IRepository
{
    IQueryable<T> GetAll<T>() where T : class, IBitemporalEntity;
}
