namespace Patterns.Creational.FactoryMethod;

sealed class ApplePayPayment : IPayment
{
    public void Pay(decimal amount)
        => WriteLine($"Successfully paid ${amount} to merchant using Apple Pay");
}
