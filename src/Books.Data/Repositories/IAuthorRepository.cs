using Books.Data.Models;

namespace Books.Data.Repositories;

public interface IAuthorRepository
{
    Task<IEnumerable<Author>> GetAllAsync(CancellationToken cancellationToken);
}
