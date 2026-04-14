namespace Examples;

class RecordsWith
{
    record class Product(string Name, string Category, decimal Price);

    static public void Run()
    {
        var apple = new Product(Name: "Apple", Category: "Fruit", Price: 4.2m);

        var banana = apple with { Name = "Banana", Price = 6.9m };

        WriteLine($"{banana.Name} is a {banana.Category} and it costs {banana.Price}");
    }
}
