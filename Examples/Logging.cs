using Microsoft.Extensions.Logging;

namespace Examples;

class Logging(ILogger<Logging> logger)
{
    public void Run()
    {
        using (logger.BeginScope("Run"))
        {
            logger.LogCritical("I am a critical message");
            logger.LogDebug("I am a debug message"); // builder.SetMinimumLevel(LogLevel.Debug)
            logger.LogError("I am an error message");
            logger.LogInformation("I am an information message");
            logger.LogTrace("I am a trace message"); // builder.SetMinimumLevel(LogLevel.Trace)
            logger.LogWarning("I am a warning message");
        }
    }
}
