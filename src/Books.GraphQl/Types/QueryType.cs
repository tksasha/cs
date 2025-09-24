namespace Books.GraphQl.Types;

public class QueryType : ObjectType<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor
            .Field(f => f.GetAllAuthorsAsync(default))
            .Type<ListType<AuthorType>>();
    }
}
