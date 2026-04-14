namespace Examples;

readonly struct Temperature(decimal celsius) : IEquatable<Temperature>
{
    readonly decimal _celsius = celsius;

    public decimal ToCelsius() => _celsius;
    public decimal ToFahrenheit() => _celsius * 9 / 5 + 32;
    public decimal ToKelvin() => _celsius + 273.15m;

    override public string ToString() => $"{_celsius:F2} °C";

    public bool Equals(Temperature other)
        => _celsius.Equals(other._celsius);

    public static bool operator ==(Temperature a, Temperature b)
        => a.ToCelsius() == b.ToCelsius();

    public static bool operator !=(Temperature a, Temperature b)
        => a.ToCelsius() != b.ToCelsius();

    public override bool Equals(object? other)
        => other is Temperature temperature && Equals(temperature);

    public override int GetHashCode()
        => _celsius.GetHashCode();

    public static bool operator >(Temperature a, Temperature b)
        => a.ToCelsius() > b.ToCelsius();

    public static bool operator <(Temperature a, Temperature b)
        => a.ToCelsius() < b.ToCelsius();
}

class Structs
{
    public static void Run()
    {
        var temperature = new Temperature(celsius: 4.2m);

        WriteLine($"Celsius = {temperature.ToCelsius():F2}");
        WriteLine($"Fahrenheit = {temperature.ToFahrenheit():F2}");
        WriteLine($"Kelvin = {temperature.ToKelvin():F2}");
        WriteLine($"String = " + temperature.ToString());

        var other = default(Temperature);

        WriteLine(other); // 0.00 °C

        var a = new Temperature(celsius: 2m);
        var b = a;

        WriteLine($"a == b = {a == b}"); // for class it would be False
    }
}
