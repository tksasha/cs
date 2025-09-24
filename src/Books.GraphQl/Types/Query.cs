using Books.Data.Models;
using Books.Core.Services;

namespace Books.GraphQl.Types;

public class Query(IAuthorService authorService)
{
    public async Task<IEnumerable<Author>> GetAllAuthorsAsync(CancellationToken cancellationToken)
        => await authorService.GetAllAsync(cancellationToken);
}
