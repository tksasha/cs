using static System.Console;

namespace Examples;

class Records
{
    struct Product : IEquatable<Product>
    {
        public string Name { get; set; }

        public readonly bool Equals(Product other)
            => Name == other.Name;

        override public readonly bool Equals(Object? other)
            => other is Product product && Equals(product);

        override public readonly int GetHashCode()
            => Name.GetHashCode();

        public static bool operator ==(Product a, Product b)
            => a.Name == b.Name;

        public static bool operator !=(Product a, Product b)
            => !(a.Name == b.Name);
    }

    record struct Beverage(string Name);

    record class Food(string Name);

    class Person
    {
        public required string Name { get; set; }
    }

    static void Change(Product product)
    {
        product.Name = "Changed Product Name";

        WriteLine($"[{nameof(Change)}] product name = {product.Name}");
    }

    static void Change(Beverage beverage)
    {
        beverage.Name = "Changed Beverage Name";

        WriteLine($"[{nameof(Change)}] beverage name = {beverage.Name}");
    }

    static void Change(Food _)
    {
        // food.Name = "Changed Food Name"; // compile error
    }

    static void Change(Person person)
    {
        person.Name = "Changed Person Name";

        WriteLine($"[{nameof(Change)}] person name = {person.Name}");
    }

    public static void Run()
    {
        var a = new Product { Name = "Apple" };
        var b = a;

        if (a == b)
        {
            WriteLine("products are same");
        }

        Change(a);

        WriteLine($"[{nameof(Run)}] product name = {a.Name}");

        var c = new Beverage(Name: "Coca Cola");
        var d = c;

        if (c == d)
        {
            WriteLine("beverages are same");
        }

        Change(c);

        WriteLine($"[{nameof(Run)}] beverage name = {c.Name}");

        var e = new Food("Hot Dog");
        var f = e;

        if (e == f)
        {
            WriteLine("food are the same");
        }

        var g = new Person { Name = "John McClane" };

        Change(g);

        WriteLine($"[{nameof(Run)}] person name = {g.Name}");
    }
}
