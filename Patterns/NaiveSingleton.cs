namespace Patterns;

sealed class NaiveSingleton
{
    public int Value { get; }

    private NaiveSingleton()
    {
        WriteLine("construct new instance");
        Value = 42;
    }

    public static NaiveSingleton Instance { get; } = new();

    public static void Run()
    {
        WriteLine(Instance.Value);
        WriteLine(Instance.Value);
    }
}
