using Books.Data.Models;

namespace Books.Data.Repositories;

public class AuthorRepository : IAuthorRepository
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
}
