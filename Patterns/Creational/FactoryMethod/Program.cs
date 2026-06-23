using Microsoft.Extensions.DependencyInjection;

namespace Patterns.Creational.FactoryMethod;

static class Program
{
    public static void Run()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddKeyedSingleton<IPayment, CreditCardPayment>(PaymentMethod.CreditCard);
        serviceCollection.AddKeyedSingleton<IPayment, PayPalPayment>(PaymentMethod.PayPal);
        serviceCollection.AddKeyedSingleton<IPayment, GooglePayPayment>(PaymentMethod.GooglePay);
        serviceCollection.AddKeyedSingleton<IPayment, ApplePayPayment>(PaymentMethod.ApplePay);

        serviceCollection.AddSingleton<PaymentFactory>();

        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var factory = serviceProvider.GetRequiredService<PaymentFactory>();

        foreach (var paymentMethod in Enum.GetValues<PaymentMethod>())
        {
            var payment = factory.Create(paymentMethod);

            payment.Pay(1000);
        }
    }
}
