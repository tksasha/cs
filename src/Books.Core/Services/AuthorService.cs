using Books.Data.Models;
using Books.Data.Repositories;

namespace Books.Core.Services;

public class AuthorService(IAuthorRepository authorRepository) : IAuthorService
{
    public async Task<IEnumerable<Author>> GetAllAsync(CancellationToken cancellationToken)
        => await authorRepository.GetAllAsync(cancellationToken);
}
