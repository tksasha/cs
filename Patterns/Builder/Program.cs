namespace Patterns.Builder;

static class Program
{
    public static void Run()
    {
        var house = HouseBuilder
            .Empty()
            .WithWall(b => b.WithColor("yellow"))
            .WithRoof(b => b.WithColor("blue"))
            .Build();

        WriteLine($"""
            floor color = {house.Floor.Color},
            wall color = {house.Wall.Color},
            roof color = {house.Roof.Color}
        """);
    }
}
