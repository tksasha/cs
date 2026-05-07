using Microsoft.Extensions.DependencyInjection;

namespace Beetles.Application.Tests;

public abstract class BaseTest
{
    protected readonly ServiceProvider _serviceProvider;

    public BaseTest()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        _serviceProvider = services.BuildServiceProvider();
    }
}
