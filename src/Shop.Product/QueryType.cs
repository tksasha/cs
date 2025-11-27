namespace Shop.Product;

public class QueryType : ObjectType<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor
            .Field(f => f.GetAllProductsAsync(default))
            .Type<ListType<ProductType>>();
    }
}
