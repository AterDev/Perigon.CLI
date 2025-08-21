using System.Text;
using Microsoft.Extensions.Logging;

namespace Ater.Web.Extension.Providers;

public class DailyFileLoggerProvider : ILoggerProvider, IDisposable
{
    private readonly string _basePath;
    private readonly Func<string, LogLevel, bool> _filter;

    private readonly Timer _cleanupTimer;
    private readonly int _retentionDays;

    public DailyFileLoggerProvider(
        string basePath,
        int retentionDays,
        Func<string, LogLevel, bool>? filter = null
    )
    {
        _basePath = basePath;
        _retentionDays = retentionDays;
        _filter = filter ?? ((category, level) => true);
        _cleanupTimer = new Timer(CleanupOldLogs, null, TimeSpan.Zero, TimeSpan.FromDays(1));
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new DailyFileLogger(categoryName, _basePath, _filter);
    }

    private void CleanupOldLogs(object? state)
    {
        try
        {
            var cutoffDate = DateTime.Today.AddDays(-_retentionDays);
            var files = Directory.GetFiles(_basePath, "log_*.log");

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch { }
                }
            }
        }
        catch { }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}

public class DailyFileLogger(
    string categoryName,
    string basePath,
    Func<string, LogLevel, bool> filter
) : ILogger
{
    private readonly Lock _fileLock = new();

    public bool IsEnabled(LogLevel logLevel)
    {
        return filter == null || filter(categoryName, logLevel);
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var logMessage = formatter(state, exception);
        var logEntry = new StringBuilder();
        logEntry.AppendFormat(
            "{0:yyyy-MM-dd HH:mm:ss.fff zzz} [{1}] [{2}] {3}",
            DateTimeOffset.Now,
            logLevel.ToString().ToUpper(),
            categoryName,
            logMessage
        );

        if (exception != null)
        {
            logEntry.Append(Environment.NewLine);
            logEntry.Append(exception.ToString());
        }

        logEntry.Append(Environment.NewLine);

        var logFileName = $"log_{DateTime.Today:yyyyMMdd}.log";
        var logFilePath = Path.Combine(basePath, logFileName);
        Directory.CreateDirectory(basePath);

        using (_fileLock.EnterScope())
        {
            File.AppendAllText(logFilePath, logEntry.ToString());
        }
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return default;
    }
}
