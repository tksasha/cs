using Microsoft.Extensions.Logging;

namespace Examples;

#pragma warning disable IDE0051

class Program
{
    static void Main()
    {
        // Nullable.Run();
        // SwitchStatement.Run();
        SwitchExpression.Run();

        using ILoggerFactory loggerFactory = LoggerFactory.Create(
            builder => builder
            // .SetMinimumLevel(LogLevel.Trace)
            .AddSimpleConsole(options => options.IncludeScopes = true));


        // RunLogging(loggerFactory);

        // RunPatternMatching(loggerFactory);
        // ParamsKeyword.Run();
    }

    static void RunLogging(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<Logging>();

        Logging logging = new(logger: logger);

        logging.Run();
    }

    static void RunPatternMatching(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<PatternMatching>();

        PatternMatching subject = new(logger: logger);

        subject.Run();
    }
}

#pragma warning restore IDE0051
