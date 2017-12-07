using Microsoft.Extensions.Logging;

namespace Remiworks.Core
{
    public static class RemiLogging
    {
        public static ILoggerFactory LoggerFactory { get; set; } = new LoggerFactory();

        public static ILogger CreateLogger<T>() =>
          LoggerFactory.CreateLogger<T>();
    }
}