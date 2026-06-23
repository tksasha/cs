using Microsoft.Extensions.DependencyInjection;

namespace Patterns.Creational.Singleton;

sealed class Di
{
    sealed class Singleton
    {
        public int Value { get; }

        public Singleton()
        {
            WriteLine("construct new instance");
            Value = 42;
        }
    }

    public static void Run()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<Singleton>();

        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var instance = serviceProvider.GetRequiredService<Singleton>();

        WriteLine(instance.Value);
        WriteLine(instance.Value);
    }
}
