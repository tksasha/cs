using Books.Data.Models;
using Books.Data.Repositories;

namespace Books.Core.Services;

public class BookService(IBookRepository bookRepository) : IBookService
{
    public async Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken)
        => await bookRepository.GetAllAsync(cancellationToken);
}
