using System.Net.Mime;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Miniverse.CLIClient;

public static class LogManager
{
    private static readonly ILoggerFactory loggerFactory;

    public static ILogger<T> CreateLogger<T>() => loggerFactory.CreateLogger<T>();
    public static readonly ILogger Global;

    static LogManager()
    {
        loggerFactory = LoggerFactory.Create(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddZLoggerConsole();
        });
        Global = loggerFactory.CreateLogger("Logger");

        AppDomain.CurrentDomain.ProcessExit += (_, _) => loggerFactory.Dispose();
    }
}
