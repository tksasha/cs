using Books.Data.Models;

namespace Books.GraphQl.Resolvers;

public class AuthorResolver
{
    public int GetAge([Parent] Author author)
        => DateTime.UtcNow.Year - author.Birthday.Year;
}
