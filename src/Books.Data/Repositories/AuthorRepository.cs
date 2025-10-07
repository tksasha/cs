using Books.Data.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Books.Data.Repositories;

public class AuthorRepository(ILogger<AuthorRepository> logger, IMemoryCache memoryCache) : IAuthorRepository
{
    private readonly IEnumerable<Author> _authors =
    [
        new(Id: "001", Name: "First Author", Birthday: new DateOnly(1970, 12, 31)),
        new(Id: "002", Name: "Second Author", Birthday: new DateOnly(1980, 12, 31)),
        new(Id: "003", Name: "Third Author", Birthday: new DateOnly(1990, 12, 31)),
    ];

    public async Task<IEnumerable<Author>> GetAllAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(0);

        return _authors;
    }

    public async Task<Author?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Get Author by id {Id}", id);

        await Task.Delay(0);

        return _authors.Where(a => a.Id == id).FirstOrDefault();
    }

    public async Task<IEnumerable<Author>> GetByIdsAsync(
        IReadOnlyList<string> ids,
        CancellationToken cancellationToken)
    {
        string cacheKey = GetCacheKey(ids);

        IEnumerable<Author>? authors;

        if (memoryCache.TryGetValue(cacheKey, out authors)
            && authors is not null)
        {
            logger.LogInformation("Found authors in the Memory Cache by key {Key}", cacheKey);

            return authors;
        }

        await Task.Delay(2000);

        logger.LogInformation("Get Authors by ids {Ids}", ids);

        authors = _authors.Where(a => ids.Contains(a.Id));

        memoryCache.Set(cacheKey, authors);

        return authors;
    }

    private string GetCacheKey(IReadOnlyList<string> ids)
        => string.Join(",", ids.OrderBy(i => i));
}
