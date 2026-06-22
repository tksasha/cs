namespace Patterns.Singleton;

sealed class Naive
{
    public int Value { get; }

    private static Naive _instance = null!;

    private Naive()
    {
        WriteLine("construct new instance");
        Value = 42;
    }

    public static Naive Instance()
    {
        _instance ??= new();

        return _instance;
    }

    public static void Run()
    {
        WriteLine(Instance().Value);
        WriteLine(Instance().Value);
    }
}
