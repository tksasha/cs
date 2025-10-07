using Books.Data.Models;
using Microsoft.Extensions.Logging;

namespace Books.Data.Repositories;

public class BookRepository(ILogger<BookRepository> logger) : IBookRepository
{
    private readonly IEnumerable<Book> _books =
    [
        new(Id: "0001", Title: "Book Number One", AuthorId: "001"),
        new(Id: "0002", Title: "Book Number Two", AuthorId: "001"),
        new(Id: "0003", Title: "Book Number Three", AuthorId: "001"),
        new(Id: "0010", Title: "Book Number Ten", AuthorId: "002"),
        new(Id: "0011", Title: "Book Number Eleven", AuthorId: "002"),
        new(Id: "0020", Title: "Book Number Twelve", AuthorId: "003"),
    ];

    public async Task<IEnumerable<Book>> GetAllByAuthorIdsAsync(
        IEnumerable<string> authorIds,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "BookRepository.GetAllByAuthorIdsAsync: authorIds = {Ids}",
            string.Join(", ", authorIds));

        await Task.Delay(2000);

        return _books.Where(b => authorIds.Contains(b.AuthorId));
    }

    public async Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(0);

        return _books;
    }
}
