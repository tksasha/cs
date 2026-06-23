namespace Patterns.Creational.Builder;

sealed class WallBuilder
{
    private string _color = Constants.DefaultColor;

    private WallBuilder()
    { }

    public static WallBuilder Empty()
        => new();

    public WallBuilder WithColor(string color)
    {
        _color = color;

        return this;
    }

    public Wall Build()
        => new() { Color = _color };
}
