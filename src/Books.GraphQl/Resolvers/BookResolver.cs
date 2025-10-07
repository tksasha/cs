using Books.Data.Models;
using Books.GraphQl.DataLoaders;

namespace Books.GraphQl.Resolvers;

public class BookResolver()
{
    public async Task<Author?> GetAuthorByIdAsync(
        [Parent] Book book,
        AuthorsByIdsDataLoader authorsByIdsDataLoader,
        CancellationToken cancellationToken)
    => await authorsByIdsDataLoader.LoadAsync(book.AuthorId, cancellationToken);
}
