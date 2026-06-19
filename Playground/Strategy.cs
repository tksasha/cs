using Microsoft.Extensions.DependencyInjection;

namespace Playground;

class Strategy
{
    interface IFeeCalculator
    {
        decimal Calculate(decimal price);
    }

    class RozetkaFeeCalculator : IFeeCalculator
    {
        private static readonly decimal _rate = 0.1M;

        public decimal Calculate(decimal price)
            => price * _rate;
    }

    class NovaPoshtaFeeCalculator : IFeeCalculator
    {
        private static readonly decimal _rate = 0.2M;

        public decimal Calculate(decimal price)
            => price * _rate;
    }

    class Delivery
    {
#pragma warning disable CA1822
        public decimal Fee(IFeeCalculator feeCalculator, decimal price)
            => feeCalculator.Calculate(price);
#pragma warning restore CA1822
    }

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
