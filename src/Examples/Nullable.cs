using Microsoft.Extensions.Logging;

namespace Examples;

class Nullable(ILogger<Nullable> logger)
{
    public void Run()
    {
        Number? number = GetNumber(isNull: true); // !!! -- w/o Number? error will be here

        using (logger.BeginScope("Run"))

        {
            logger.LogInformation("number = {Number}", number);
        }
    }

    static Number? GetNumber(bool isNull) => isNull ? null : new Number { };

    class Number
    {
        public override string ToString() => "Hello, I'm a Number";
    }
}
