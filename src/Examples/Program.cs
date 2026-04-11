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

        // RunNullable(loggerFactory);
        // RunSwitchStatement(loggerFactory);
        // RunSwitchExpression(loggerFactory);
        // RunPatternMatching(loggerFactory);
        ParamsKeyword.Run();
    }

    static void RunNullable(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<Nullable>();

        Nullable subject = new(logger: logger);

        subject.Run();
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

    static void RunPatternMatching(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<PatternMatching>();

        PatternMatching subject = new(logger: logger);

        subject.Run();
    }
}

#pragma warning restore IDE0051
