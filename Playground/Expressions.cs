using System.Linq.Expressions;

namespace Playground;

static class Expressions
{
    delegate void DelegateOne(string name);

    public static void Run()
    {
        Action<bool> one = One;

        one(false);

        Action two = Two;

        two();

        DelegateOne third = Third;

        third("Bruce Wayne");

        Func<int, int, int> sum = Sum;

        WriteLine($"sum = {sum(42, 69)}");

        Predicate<int> predicate = IsAdult;

        WriteLine($"42 is adult = {predicate(42)}");

        Action<string> show = delegate (string output) { WriteLine(output); };

        show("lorem ipsum ...");

        Expression<Action<bool>> expression = (flag) => One(flag);

        if (expression.Body is MethodCallExpression body)
        {
            WriteLine($"method name = {body.Method.Name}");
        }

        Action<bool> compiled = expression.Compile();

        compiled(false);
    }

    static void One(bool flag)
    {
        WriteLine($"I'm One with flag = {flag}.");
    }

    static void Two()
    {
        WriteLine("I'm Two.");
    }

    static void Third(string name)
    {
        WriteLine($"I'm Third and name = {name}");
    }

    static int Sum(int a, int b)
        => a + b;

    static bool IsAdult(int age)
        => age >= 18;
}
