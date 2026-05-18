using Beetles.Application;
using Beetles.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Beetles.Application.Tests.Database;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private const string ConnectionStringKey = "ConnectionStrings:Development";

    private readonly PostgreSqlContainer _postgresqlContainer = new PostgreSqlBuilder("postgres:18").Build();

    private ServiceProvider _serviceProvider = null!;

    public string ConnectionString => _postgresqlContainer.GetConnectionString();
    public TestTimeProvider TimeProvider { get; } = new();

    public IServiceScope CreateScope() => _serviceProvider.CreateScope();

    public async Task InitializeAsync()
    {
        await _postgresqlContainer.StartAsync(CancellationToken.None);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [ConnectionStringKey] = ConnectionString,
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Warning));
        services.AddInfrastructure(config);
        services.AddApplication();
        services.AddSingleton<TimeProvider>(TimeProvider);

        _serviceProvider = services.BuildServiceProvider();

        using var scope = CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await ctx.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
        await _postgresqlContainer.StopAsync(CancellationToken.None);
    }

    public async Task ResetAsync()
    {
        // Wipe the walls table and reset the identity sequence so every test starts from a
        // clean, deterministic state. We use TRUNCATE ... RESTART IDENTITY so the next inserted
        // row gets Id = 1 regardless of how many rows previous tests inserted.
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "TRUNCATE TABLE walls RESTART IDENTITY CASCADE;";
        await command.ExecuteNonQueryAsync();

        TimeProvider.Reset();
    }
}
