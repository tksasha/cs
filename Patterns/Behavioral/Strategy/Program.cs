using Microsoft.Extensions.DependencyInjection;

namespace Patterns.Behavioral.Strategy;

static class Program
{
    public static void Run()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<RozetkaFeeCalculator>();
        serviceCollection.AddSingleton<NovaPoshtaFeeCalculator>();
        serviceCollection.AddSingleton<Delivery>();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var delivery = serviceProvider.GetRequiredService<Delivery>();

        var rozetkaFee = delivery.Fee(
            serviceProvider.GetRequiredService<RozetkaFeeCalculator>(), 100M);

        WriteLine($"rozetka fee = {rozetkaFee:C}");

        var novaPoshtaFee = delivery.Fee(
            serviceProvider.GetRequiredService<NovaPoshtaFeeCalculator>(), 100M);

        WriteLine($"nova poshta fee = {novaPoshtaFee:C}");
    }
}
