using Microsoft.Extensions.DependencyInjection;

namespace Patterns.SimpleFactory;

enum PaymentMethod
{
    CreditCard,
    PayPal,
    GooglePay,
    ApplePay,
}

interface IPayment
{
    void Pay(decimal amount);
}

sealed class CreditCardPayment : IPayment
{
    public void Pay(decimal amount)
        => WriteLine($"Successfully paid ${amount} to merchant using Credit Card");
}

sealed class PayPalPayment : IPayment
{
    public void Pay(decimal amount)
        => WriteLine($"Successfully paid ${amount} to merchant using PayPal");
}

sealed class GooglePayPayment : IPayment
{
    public void Pay(decimal amount)
        => WriteLine($"Successfully paid ${amount} to merchant using Google Pay");
}

sealed class ApplePayPayment : IPayment
{
    public void Pay(decimal amount)
        => WriteLine($"Successfully paid ${amount} to merchant using Apple Pay");
}

sealed class PaymentFactory(IServiceProvider serviceProvider)
{
    public IPayment Create(PaymentMethod paymentMethod)
        => serviceProvider.GetRequiredKeyedService<IPayment>(paymentMethod);
}

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
