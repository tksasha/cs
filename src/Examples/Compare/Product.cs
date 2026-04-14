namespace Examples.Compare;

record class Product(int Id, string Name) : IComparable<Product>
{
    public int CompareTo(Product? other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return Name.CompareTo(other.Name);
    }
}

class ProductIdComparer : IComparer<Product>
{
    public int Compare(Product? a, Product? b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        return a.Id.CompareTo(b.Id);
    }
}

class Test
{
    public static void Run()
    {
        List<Product> products = [
            new Product(Id: 1, Name: "Bread"),
            new Product(Id: 4, Name: "Meat"),
            new Product(Id: 2, Name: "Beer"),
            new Product(Id: 3, Name: "Wine"),
        ];

        WriteLine("sorting by name");

        products.Sort();

        foreach (var product in products)
        {
            WriteLine($"{product.Id}, {product.Name}");
        }

        WriteLine("sorting by id");

        // products.Sort(new ProductIdComparer());
        // or
        products.Sort((a, b) => a.Id.CompareTo(b.Id));

        foreach (var product in products)
        {
            WriteLine($"{product.Id}, {product.Name}");
        }
    }
}
