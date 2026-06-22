namespace Patterns;

sealed class Singleton
{
    public int Value { get; }

    private Singleton()
    {
        WriteLine("construct new instance");
        Value = 42;
    }

    public static Singleton Instance { get; } = new();

    public static void Run()
    {
        WriteLine(Instance.Value);
        WriteLine(Instance.Value);
    }
}
