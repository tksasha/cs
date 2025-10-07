using Books.Data.Models;
using Books.Data.Repositories;

namespace Books.GraphQl.DataLoaders;

public class AuthorsByIdsDataLoader : BatchDataLoader<string, Author>
{
    private readonly IAuthorRepository _authorRepository;

    public AuthorsByIdsDataLoader(
        IAuthorRepository authorRepository,
        IBatchScheduler batchScheduler,
        DataLoaderOptions options) : base(batchScheduler, options)
    {
        _authorRepository = authorRepository;
    }

    protected override async Task<IReadOnlyDictionary<string, Author>> LoadBatchAsync(
        IReadOnlyList<string> ids,
        CancellationToken cancellationToken)
    {
        IEnumerable<Author> authors = await _authorRepository.GetByIdsAsync(ids, cancellationToken);

        return authors.ToDictionary(a => a.Id, a => a);
    }
}
