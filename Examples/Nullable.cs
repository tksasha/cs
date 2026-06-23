namespace Examples;

static class Nullable
{
    public static void Run()
    {
        Number? number = GetNumber(isNull: true); // !!! -- w/o Number? error will be here

        WriteLine($"[{nameof(Nullable)}.{nameof(Run)}] number = {number}");
    }

    static Number? GetNumber(bool isNull) => isNull ? null : new Number { };

    sealed class Number
    {
        public override string ToString() => "Hello, I'm a Number";
    }
}
