using Books.Data.Models;

namespace Books.Core.Services;

public interface IAuthorService
{
    Task<IEnumerable<Author>> GetAllAsync(CancellationToken cancellationToken);
}
