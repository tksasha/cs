namespace Examples;

interface IRepository<T>
{
    void Add(T item);
    T? GetById(int id);
    IEnumerable<T> GetAll();

    bool TryGetById(int id, out T item);
}

record class Product(int Id, string Name);

class ProductRepository : IRepository<Product>
{
    readonly List<Product> _repository = [];

    public void Add(Product item)
    {
        _repository.Add(item);
    }

    public Product? GetById(int id)
        => _repository.FirstOrDefault(p => p.Id == id);

    public IEnumerable<Product> GetAll()
        => _repository;

    public bool TryGetById(int id, out Product product)
    {
        product = GetById(id)!;

        return product is not null;
    }

    public static void Run()
    {
        var products = new ProductRepository();

        products.Add(new Product(Id: 1, Name: "Bread"));
        products.Add(new Product(Id: 2, Name: "Meat"));
        products.Add(new Product(Id: 3, Name: "Beer"));
        products.Add(new Product(Id: 4, Name: "Wine"));

        if (products.TryGetById(4, out Product wine))
        {
            WriteLine($"wine.Id = {wine.Id}, wine.Name = {wine.Name}");
        }
    }
}
