using Books.Data.Models;
using Books.Data.Repositories;
using HotChocolate.Resolvers;

namespace Books.GraphQl.Resolvers;

public class AuthorResolver(IBookRepository bookRepository)
{
    public int GetAge([Parent] Author author)
        => DateTime.UtcNow.Year - author.Birthday.Year;

    public async Task<IEnumerable<Book>> GetBooksUnoptimalAsync(
        [Parent] Author author,
        CancellationToken cancellationToken)
        => await bookRepository.GetAllByAuthorIdsAsync([author.Id], cancellationToken);

    public async Task<IEnumerable<Book>> GetBooksByGroupDataLoaderAsync(
        [Parent] Author author,
        IResolverContext resolverContext,
    CancellationToken cancellationToken)
        => await resolverContext.GroupDataLoader<string, Book>(
            async (authorIds, cancellationToken) =>
            {
                IEnumerable<Book> books = await bookRepository.GetAllByAuthorIdsAsync(authorIds, cancellationToken);

                return books.ToLookup(b => b.AuthorId);
            }).LoadAsync(author.Id);
}
