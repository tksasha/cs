namespace Patterns.Creational.FactoryMethod;

sealed class CreditCardPayment : IPayment
{
    public void Pay(decimal amount)
        => WriteLine($"Successfully paid ${amount} to merchant using Credit Card");
}
