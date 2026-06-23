namespace Patterns.Creational.Builder;

sealed class HouseBuilder
{
    private readonly FloorBuilder _floorBuilder = FloorBuilder.Empty();
    private readonly WallBuilder _wallBuilder = WallBuilder.Empty();
    private readonly RoofBuilder _roofBuilder = RoofBuilder.Empty();

    private HouseBuilder()
    { }

    public static HouseBuilder Empty()
        => new();

    public HouseBuilder WithFloor(Action<FloorBuilder> builder)
    {
        builder(_floorBuilder);

        return this;
    }

    public HouseBuilder WithWall(Action<WallBuilder> builder)
    {
        builder(_wallBuilder);

        return this;
    }

    public HouseBuilder WithRoof(Action<RoofBuilder> builder)
    {
        builder(_roofBuilder);

        return this;
    }

    public House Build()
        => new()
        {
            Floor = _floorBuilder.Build(),
            Wall = _wallBuilder.Build(),
            Roof = _roofBuilder.Build(),
        };
}
