using Books.Data.Models;

namespace Books.Data.Repositories;

public interface IAuthorRepository
{
    Task<IEnumerable<Author>> GetAllAsync(CancellationToken cancellationToken);

    Task<Author?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task<IEnumerable<Author>> GetByIdsAsync(IReadOnlyList<string> ids, CancellationToken cancellationToken);
}
