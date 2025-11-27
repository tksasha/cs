namespace Shop.Product;

public interface IService
{
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken);
}

public class Service(IRepository productRepository) : IService
{
    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken)
        => await productRepository.GetAllAsync(cancellationToken);
}
