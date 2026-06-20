using System.Globalization;

namespace Playground;

static class Formattable
{
    sealed class Author : IFormattable
    {
        public required string Name { get; set; }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                format = "N";
            }

            return format.ToUpperInvariant() switch
            {
                "N" => Name,
                _ => throw new FormatException($"The {format} format specifier is not supported."),
            };
        }

        public override string ToString()
            => ToString("N", CultureInfo.CurrentCulture);
    }

    sealed class Book : IFormattable
    {
        public required string Title { get; set; }
        public required Author Author { get; set; }
        public decimal Price { get; set; }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                format = "D";
            }

            formatProvider ??= CultureInfo.CurrentCulture;

            return format.ToUpperInvariant() switch
            {
                "D" => $"{Title} by {Author.Name}, {Price.ToString("C", formatProvider)}",
                _ => throw new FormatException($"The {format} format specifier is not supported."),
            };
        }

        public override string ToString()
            => ToString("D", CultureInfo.CurrentCulture);
    }

    public static void Run()
    {
        var author = new Author { Name = "Sir Arthur Conan Doyle" };

        WriteLine(author);

        var book = new Book { Title = "A Study in Scarlet", Price = 10.39M, Author = author };

        WriteLine(book);
    }
}
