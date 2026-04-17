using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using EFDemo.Models;
using Microsoft.EntityFrameworkCore.Design;

namespace EFDemo;

public class DatabaseContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
{
    public DbSet<Blog> Blogs { get; set; } = default!;
    public DbSet<Grade> Grades { get; set; } = default!;
    public DbSet<Post> Posts { get; set; } = default!;
}

public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseNpgsql(configuration.GetConnectionString("Development"))
            .Options;

        return new DatabaseContext(options);
    }
}
