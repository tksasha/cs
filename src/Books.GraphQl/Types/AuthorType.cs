using Books.Data.Models;
using Books.GraphQl.Resolvers;

namespace Books.GraphQl.Types;

public class AuthorType : ObjectType<Author>
{
    protected override void Configure(IObjectTypeDescriptor<Author> descriptor)
    {
        descriptor
            .Field(f => f.Id)
            .Type<IntType>();

        descriptor
            .Field(f => f.Name)
            .Type<StringType>();

        descriptor
            .Field("age")
            .Type<IntType>()
            .ResolveWith<AuthorResolver>(r => r.GetAge(default!));

        descriptor
            .Field("booksUnoptimal")
            .Type<ListType<BookType>>()
            .ResolveWith<AuthorResolver>(r => r.GetBooksUnoptimalAsync(default!, default));

        descriptor
            .Field("booksByDataLoader")
            .Type<ListType<BookType>>()
            .ResolveWith<AuthorResolver>(r => r.GetBooksByDataLoaderAsync(default!, default!, default!, default));
    }
}
