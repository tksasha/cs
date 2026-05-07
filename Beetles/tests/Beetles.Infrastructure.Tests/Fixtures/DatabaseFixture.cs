using Beetles.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Testcontainers.PostgreSql;

namespace Beetles.Infrastructure.Tests.Fixtures;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private static readonly PostgreSqlContainer PostgreSqlContainer = new PostgreSqlBuilder("postgres:18").Build();

    public DatabaseContext DatabaseContext { get; private set; } = null!;

    public Mock<TimeProvider> TimeProviderMock { get; } = new();

    public IBitemporalRepository BitemporalRepository { get; private set; } = null!;

    private IServiceScope? _scope;

    public async Task InitializeAsync()
    {
        await PostgreSqlContainer.StartAsync(CancellationToken.None);

        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Development"] = PostgreSqlContainer.GetConnectionString(),
            }).Build();

        services.AddSingleton(TimeProviderMock.Object);

        services.AddSingleton<ILoggerFactory, LoggerFactory>();

        services.AddInfrastructure(configuration);

        _scope = services.BuildServiceProvider().CreateScope();

        var serviceProvider = _scope.ServiceProvider;

        DatabaseContext = serviceProvider.GetRequiredService<DatabaseContext>();

        await DatabaseContext.Database.MigrateAsync(CancellationToken.None);

        BitemporalRepository = serviceProvider.GetRequiredService<IBitemporalRepository>();
    }

    public async Task DisposeAsync()
    {
        _scope?.Dispose();

        await PostgreSqlContainer.StopAsync(CancellationToken.None);
    }
}
