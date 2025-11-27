namespace Shop.Product;

public class ProductType : ObjectType<Product>
{
    protected override void Configure(IObjectTypeDescriptor<Product> descriptor)
    {
        descriptor
            .Field(f => f.Id)
            .Type<StringType>();

        descriptor
            .Field(f => f.Name)
            .Type<StringType>();
    }
}
