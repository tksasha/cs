using Books.Data.Models;
using Books.GraphQl.Resolvers;

namespace Books.GraphQl.Types;

public class BookType : ObjectType<Book>
{
    protected override void Configure(IObjectTypeDescriptor<Book> descriptor)
    {
        descriptor
            .Field(f => f.Id)
            .Type<StringType>();

        descriptor
            .Field(f => f.Title)
            .Type<StringType>();

        descriptor
            .Field(f => f.AuthorId)
            .Type<StringType>();

        descriptor
            .Field("author")
            .Type<AuthorType>()
            .ResolveWith<BookResolver>(r => r.GetAuthorByIdAsync(default!, default!, default));
    }
}
