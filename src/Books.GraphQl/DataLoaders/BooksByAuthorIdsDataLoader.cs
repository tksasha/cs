using Books.Data.Models;
using Books.Data.Repositories;

namespace Books.GraphQl.DataLoaders;

public class BooksByAuthorIdsDataLoader : BatchDataLoader<string, Book[]>
{
    private readonly IBookRepository _bookRepository;

    public BooksByAuthorIdsDataLoader(
        IBookRepository bookRepository,
        IBatchScheduler batchScheduler,
        DataLoaderOptions dataLoaderOptions) : base(batchScheduler, dataLoaderOptions)
    {
        _bookRepository = bookRepository;
    }
    protected override async Task<IReadOnlyDictionary<string, Book[]>> LoadBatchAsync(
        IReadOnlyList<string> authorIds,
        CancellationToken cancellationToken)
    {
        IEnumerable<Book> books = await _bookRepository.GetAllByAuthorIdsAsync(authorIds, cancellationToken);

        return books
            .GroupBy(book => book.AuthorId)
            .Select(group => new { AuthorId = group.Key, Books = group.ToArray() })
            .ToDictionary(x => x.AuthorId, x => x.Books);
    }
}
