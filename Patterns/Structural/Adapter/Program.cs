namespace Patterns.Structural.Adapter;

interface IGettable
{
    decimal[] Get();
}

sealed class LegacyGettable
{
#pragma warning disable CA1822
    public int[] Get()
        => [37, 42, 69];
#pragma warning restore CA1822
}

sealed class GettableAdapter(LegacyGettable gettable) : IGettable
{
    public decimal[] Get()
        => [.. gettable.Get().Select(i => (decimal)i)];
}

static class Program
{
    public static void Run()
    {
        var legacyGettable = new LegacyGettable();

        var gettable = new GettableAdapter(legacyGettable);

        var numbers = gettable.Get();

        WriteLine(string.Join(", ", numbers));
    }
}
