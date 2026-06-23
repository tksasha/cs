namespace Patterns.Strategy;

sealed class RozetkaFeeCalculator : IFeeCalculator
{
    private const decimal _rate = 0.1M;

    public decimal Calculate(decimal price)
        => price * _rate;
}
