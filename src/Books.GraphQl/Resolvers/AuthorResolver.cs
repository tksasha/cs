using Books.Data.Models;
using Books.Data.Repositories;
using HotChocolate.Resolvers;
using Books.GraphQl.DataLoaders;

namespace Books.GraphQl.Resolvers;

public class AuthorResolver(IBookRepository bookRepository)
{
    public int GetAge([Parent] Author author)
        => DateTime.UtcNow.Year - author.Birthday.Year;

    public async Task<IEnumerable<Book>> GetBooksUnoptimalAsync(
        [Parent] Author author,
        CancellationToken cancellationToken)
        => await bookRepository.GetAllByAuthorIdsAsync([author.Id], cancellationToken);

    public async Task<IEnumerable<Book>> GetBooksByDataLoaderAsync(
        [Parent] Author author,
        IResolverContext resolverContext,
        BooksByAuthorIdsDataLoader dataLoader,
        CancellationToken cancellationToken)
        => await dataLoader.LoadAsync(author.Id, cancellationToken) ?? [];
}
