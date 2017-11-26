using Microsoft.Extensions.Logging;

namespace RabbitFramework
{
    public static class RabbitLogging
    {
        public static ILoggerFactory LoggerFactory { get; set; } = new LoggerFactory();

        public static ILogger CreateLogger<T>() =>
          LoggerFactory.CreateLogger<T>();
    }
}