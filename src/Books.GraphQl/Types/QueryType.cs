namespace Books.GraphQl.Types;

public class QueryType : ObjectType<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor
            .Field(f => f.GetAuthorsAsync(default))
            .Type<ListType<AuthorType>>();

        descriptor
            .Field(f => f.GetBooksAsync(default))
            .Type<ListType<BookType>>();
    }
}
