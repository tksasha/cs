using Books.Data.Models;
using Books.Core.Services;

namespace Books.GraphQl.Types;

public class Query(IAuthorService authorService, IBookService bookService)
{
    public async Task<IEnumerable<Author>> GetAuthorsAsync(CancellationToken cancellationToken)
        => await authorService.GetAllAsync(cancellationToken);

    public async Task<IEnumerable<Book>> GetBooksAsync(CancellationToken cancellationToken)
        => await bookService.GetAllAsync(cancellationToken);
}
