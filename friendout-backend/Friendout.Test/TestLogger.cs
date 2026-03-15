using System;
using Microsoft.Extensions.Logging;

namespace Friendout.Test;

/// <summary>
/// Simple no-op logger implementation used for unit tests.
/// </summary>
/// <typeparam name="T">Type being logged.</typeparam>
public sealed class TestLogger<T> : ILogger<T>
{
    public static readonly TestLogger<T> Instance = new();

    private TestLogger()
    {
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => false;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        // Intentionally no-op for tests
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}

