using Microsoft.Extensions.Logging;

namespace Examples;

#pragma warning disable IDE0051

class Program
{
    static void Main()
    {
        // Nullable.Run();
        // SwitchStatement.Run();
        // SwitchExpression.Run();
        // PatternMatching.Run();
        // ParamsKeyword.Run();
        RunLogging();
    }

    static void RunLogging()
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(
            builder => builder
            // .SetMinimumLevel(LogLevel.Trace)
            .AddSimpleConsole(options => options.IncludeScopes = true));

        var logger = loggerFactory.CreateLogger<Logging>();

        Logging logging = new(logger: logger);

        logging.Run();
    }
}

#pragma warning restore IDE0051
