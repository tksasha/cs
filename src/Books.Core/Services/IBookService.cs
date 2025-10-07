using Books.Data.Models;

namespace Books.Core.Services;

public interface IBookService
{
    Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken);
}
