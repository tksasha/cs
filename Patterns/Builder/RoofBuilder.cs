namespace Patterns.Builder;

sealed class RoofBuilder
{
    private string _color = Constants.DefaultColor;

    private RoofBuilder()
    { }

    public static RoofBuilder Empty()
        => new();

    public RoofBuilder WithColor(string color)
    {
        _color = color;

        return this;
    }

    public Roof Build()
        => new() { Color = _color };
}
