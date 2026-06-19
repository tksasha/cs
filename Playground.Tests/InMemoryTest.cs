using Microsoft.EntityFrameworkCore;

namespace Playground.Tests;

sealed class Book
{
    public int Id { get; set; }
    public required string Title { get; set; }
}

sealed class PlaygroundDbContext(DbContextOptions<PlaygroundDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
}

public sealed class InMemoryTest : IDisposable
{
    private readonly PlaygroundDbContext _dbContext = null!;

    public InMemoryTest()
    {
        var options = new DbContextOptionsBuilder<PlaygroundDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        _dbContext = new PlaygroundDbContext(options);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Add_ShouldAddBook()
    {
        var book = new Book { Title = "A Study in Scarlet" };

        _dbContext.Add(book);

        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var result = await _dbContext.Books.FirstAsync(CancellationToken.None);

        Assert.Multiple(
            () => Assert.True(result.Id > 0),
            () => Assert.Equal("A Study in Scarlet", result.Title)
        );
    }
}
