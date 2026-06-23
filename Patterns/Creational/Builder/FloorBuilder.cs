namespace Patterns.Creational.Builder;

sealed class FloorBuilder
{
    private string _color = Constants.DefaultColor;

    private FloorBuilder()
    { }

    public static FloorBuilder Empty()
        => new();

    public FloorBuilder WithColor(string color)
    {
        _color = color;

        return this;
    }

    public Floor Build()
        => new() { Color = _color };
}
