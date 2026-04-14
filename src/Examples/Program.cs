using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Examples;

class Program
{
    static void Main()
    {
        // Nullable.Run();
        // SwitchStatement.Run();
        // SwitchExpression.Run();
        // PatternMatching.Run();
        // ParamsKeyword.Run();
        // RunLogging();
        // LocalFunction.Run();
        // Structs.Run();
        // Records.Run();
        // RecordsWith.Run();
        // Classes.BankAccount.Run();
        // Classes.Test.Run();
        // Classes.Animal.Run();
        ProductRepository.Run();
    }

#pragma warning disable IDE0051
    static void RunLogging()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        using ILoggerFactory loggerFactory = LoggerFactory.Create(
            builder => builder
            // .SetMinimumLevel(LogLevel.Trace)
            .AddConfiguration(config.GetSection("Logging"))
            .AddSimpleConsole(options => options.IncludeScopes = true));

        var logger = loggerFactory.CreateLogger<Logging>();

        Logging logging = new(logger: logger);

        logging.Run();
    }
#pragma warning restore IDE0051
}
