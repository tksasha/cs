using Microsoft.Extensions.Logging;

namespace Examples;

#pragma warning disable IDE0051

class Program
{
    static void Main()
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(
            builder => builder.AddSimpleConsole(
                options => options.IncludeScopes = true));

        // Nullable.Run();

        // RunSwitchStatement(loggerFactory);
        RunSwitchExpression(loggerFactory);
    }

    static void RunSwitchStatement(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<SwitchStatement>();

        SwitchStatement subject = new(logger: logger);

        subject.Run();
    }

    static void RunSwitchExpression(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<SwitchStatement>();

        SwitchStatement subject = new(logger: logger);

        subject.Run();
    }
}

#pragma warning restore IDE0051
