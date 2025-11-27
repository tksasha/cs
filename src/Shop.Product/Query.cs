namespace Shop.Product;

public class Query(IService service)
{
    public async Task<IEnumerable<Product>> GetAllProductsAsync(CancellationToken cancellationToken)
        => await service.GetAllAsync(cancellationToken);
}
