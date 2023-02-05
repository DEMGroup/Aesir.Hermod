using Microsoft.Extensions.Logging;

namespace Aesir.Hermod.Logging;

internal class NullLogger<T> : ILogger<T>
{
    public static readonly NullLogger<T> Instance = new NullLogger<T>();

    IDisposable? ILogger.BeginScope<TState>(TState state) => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => false;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter) { }
}

internal class NullScope : IDisposable
{
    public static readonly NullScope Instance = new NullScope();

    public void Dispose() { }
}
