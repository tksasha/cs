namespace Patterns.Creational.Prototype;

interface IPrototype<T> where T : class
{
    T DeepClone();
}

sealed class Book : IPrototype<Book>
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public int[] Ratings { get; set; } = [];

    public Book DeepClone()
    {
        var book = (Book)MemberwiseClone();

        book.Ratings = (int[])Ratings.Clone();

        return book;
    }
}

static class Program
{
    public static void Run()
    {
        var book1 = new Book { Id = 1, Title = "Book One", Ratings = [1, 2, 3] };

        var book2 = book1.DeepClone();

        WriteLine($"book1 ratings = {string.Join(",", book1.Ratings)}");
        WriteLine($"book2 ratings = {string.Join(",", book2.Ratings)}");

        book2.Ratings[1] = 9;

        WriteLine($"book1 ratings = {string.Join(",", book1.Ratings)}");
        WriteLine($"book2 ratings = {string.Join(",", book2.Ratings)}");
    }
}
