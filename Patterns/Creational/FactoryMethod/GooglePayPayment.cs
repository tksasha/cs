namespace Patterns.Creational.FactoryMethod;

sealed class GooglePayPayment : IPayment
{
    public void Pay(decimal amount)
        => WriteLine($"Successfully paid ${amount} to merchant using Google Pay");
}
