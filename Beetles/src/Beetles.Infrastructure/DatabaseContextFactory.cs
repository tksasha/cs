using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Beetles.Infrastructure;

public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<DatabaseContextFactory>()
            .Build();

        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseNpgsql(configuration.GetConnectionString("Development"))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new DatabaseContext(options);
    }
}
