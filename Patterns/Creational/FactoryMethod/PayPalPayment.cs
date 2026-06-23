namespace Patterns.Creational.FactoryMethod;

sealed class PayPalPayment : IPayment
{
    public void Pay(decimal amount)
        => WriteLine($"Successfully paid ${amount} to merchant using PayPal");
}
