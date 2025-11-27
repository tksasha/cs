namespace Shop.Product;

public interface IRepository
{
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken);
}

public class Repository : IRepository
{
    private readonly IEnumerable<Product> _products = [
        new(Id: "6928450a844b463ab4b512ba", Name: "Product Number One"),
        new(Id: "69284523da1ac0be3a25bfb7", Name: "Product Number Two"),
    ];
    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(0);

        return _products;
    }
}
