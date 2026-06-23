namespace Patterns.Behavioral.Strategy;

sealed class Delivery
{
#pragma warning disable CA1822, S2325
    public decimal Fee(IFeeCalculator feeCalculator, decimal price)
        => feeCalculator.Calculate(price);
#pragma warning restore CA1822, S2325
}
