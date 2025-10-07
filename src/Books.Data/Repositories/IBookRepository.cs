using Books.Data.Models;

namespace Books.Data.Repositories;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllByAuthorIdsAsync(IEnumerable<string> authorIds, CancellationToken cancellationToken);

    Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken);
}
