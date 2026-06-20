namespace Playground;

#pragma warning disable S125, S1144
static class Delegates
{
    static int Sum(int a, int b)
        => a + b;

    static void Inspect(object? obj)
    {
        string message = obj switch
        {
            null => "null",
            _ => obj.ToString()!
        };

        WriteLine($"object = {message}");
    }

    delegate int Summarizer(int a, int b);

    delegate void Inspector(object? obj);

    public static void Run()
    {
        // var sum = new Summary(Sum);
        // Summary sum = Sum;
        Func<int, int, int> sum = Sum;

        var result = sum(3, 7);

        WriteLine($"result = {result}");

        // var inspect = new Inspector(Inspect);
        // Inspector inspect = Inspect;
        Action<object?> inspect = Inspect;

        inspect(null);
        inspect(37);
    }
}
#pragma warning restore S125, S1144
