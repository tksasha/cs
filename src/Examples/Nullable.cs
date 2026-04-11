using static System.Console;

namespace Examples;

class Nullable
{
    public static void Run()
    {
        Number number = GetNumber(isNull: true); // !!! -- error is here

        WriteLine($"[Nullable.Run] number = {number}");
    }

    static Number? GetNumber(bool isNull) => isNull ? null : new Number { };

    class Number
    {
        public override string ToString() => "Hello, I'm a Number";
    }
}
