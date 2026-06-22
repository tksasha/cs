namespace Patterns.Singleton;

sealed class MyNaive
{
    public int Value { get; }

    private MyNaive()
    {
        WriteLine("construct new instance");
        Value = 42;
    }

    public static MyNaive Instance { get; } = new();

    public static void Run()
    {
        WriteLine(Instance.Value);
        WriteLine(Instance.Value);
    }
}
