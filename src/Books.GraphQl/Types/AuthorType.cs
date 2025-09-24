using Books.Data.Models;

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
    }
}
