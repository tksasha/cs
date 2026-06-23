namespace Patterns.Strategy;

interface IFeeCalculator
{
    decimal Calculate(decimal price);
}
