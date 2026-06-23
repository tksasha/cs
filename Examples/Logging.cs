using Microsoft.Extensions.Logging;

namespace Examples;

sealed partial class Logging(ILogger<Logging> logger)
{
    static class EventIds
    {
        public const int LogCriticalMessage = 1001;
        public const int LogDebugMessage = 1002;
        public const int Error = 1003;
        public const int LogInformationMessage = 1004;
        public const int LogTraceMessage = 1005;
        public const int LogWarningMessage = 1006;
    }

    public void Run()
    {
        using (logger.BeginScope("Run"))
        {
            LogCriticalMessage(logger);
            LogDebugMessage(logger); // builder.SetMinimumLevel(LogLevel.Debug)
            LogErrorMessageWithArgument(logger, "something went wrong");
            LogInformationMessage(logger);
            LogTraceMessage(logger); // builder.SetMinimumLevel(LogLevel.Trace)
            LogWarningMessage(logger);
        }
    }

    [LoggerMessage(
        EventId = EventIds.LogCriticalMessage,
        Level = LogLevel.Critical,
        Message = "I am a critical message")]
    private static partial void LogCriticalMessage(ILogger logger);

    [LoggerMessage(
        EventId = EventIds.LogDebugMessage,
        Level = LogLevel.Debug,
        Message = "I am a debug message")]
    private static partial void LogDebugMessage(ILogger logger);

    [LoggerMessage(
        EventId = EventIds.Error,
        Level = LogLevel.Warning,
        Message = "Failed to continue because {Reason}")]
    private static partial void LogErrorMessageWithArgument(ILogger logger, string reason);

    [LoggerMessage(
        EventId = EventIds.LogInformationMessage,
        Level = LogLevel.Information,
        Message = "I am an information message")]
    private static partial void LogInformationMessage(ILogger logger);

    [LoggerMessage(
        EventId = EventIds.LogTraceMessage,
        Level = LogLevel.Trace,
        Message = "I am a trace message")]
    private static partial void LogTraceMessage(ILogger logger);

    [LoggerMessage(
        EventId = EventIds.LogWarningMessage,
        Level = LogLevel.Warning,
        Message = "I am a warning message")]
    private static partial void LogWarningMessage(ILogger logger);
}
