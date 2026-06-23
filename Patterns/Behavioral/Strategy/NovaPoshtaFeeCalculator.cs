namespace Patterns.Behavioral.Strategy;

sealed class NovaPoshtaFeeCalculator : IFeeCalculator
{
    private const decimal _rate = 0.2M;

    public decimal Calculate(decimal price)
        => price * _rate;
}
