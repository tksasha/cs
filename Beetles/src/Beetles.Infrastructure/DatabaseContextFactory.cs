using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Beetles.Infrastructure;

public class DatabaseContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<DatabaseContextFactory>()
            .Build();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(configuration.GetConnectionString("Development"))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new ApplicationDbContext(options);
    }
}
