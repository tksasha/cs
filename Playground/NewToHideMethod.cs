namespace Playground;

class NewToHideMethod
{
    class Person
    {
        internal required string Name { get; init; }
        internal virtual int Level { get; init; }
    }

    class Player : Person
    {
        internal new int Level { get; init; }
    }

    public static void Run()
    {
        var player = new Player { Name = "Player Two", Level = 7 };

        WriteLine($"{player.Name}, {player.Level}");
    }
}