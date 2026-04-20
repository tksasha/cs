using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace EFDemo;

public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseNpgsql(configuration.GetConnectionString("Development"))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new DatabaseContext(options);
    }
}
