using Microsoft.Extensions.DependencyInjection;

namespace Beetles.Application.Tests;

public abstract class AbstractTest
{
    protected readonly ServiceProvider _serviceProvider;

    public AbstractTest()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        _serviceProvider = services.BuildServiceProvider();
    }
}
