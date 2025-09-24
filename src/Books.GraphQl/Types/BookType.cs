using Books.Data.Models;

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
    }
}
