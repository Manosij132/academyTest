using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Staffing.Shared.Logging
{
    public class FileLogger : ILogger
    {
        protected readonly FileLoggerProvider _loggerFileProvider;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // Semaphore for controlling access

        public FileLogger([NotNull] FileLoggerProvider loggerFileProvider)
        {
            _loggerFileProvider = loggerFileProvider;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            // Call the async logging method
            _ = LogAsync(logLevel, state, exception, formatter);
        }

        private async Task LogAsync<TState>(LogLevel logLevel, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            #pragma warning disable SCS0031
            var fullFilePath = Path.Combine(_loggerFileProvider.Options.FolderPath,
                _loggerFileProvider.Options.FilePath.Replace("{date}", DateTimeOffset.UtcNow.ToString("yyyyMMdd")));
            var logRecord = string.Format("{0} [{1}] {2} {3}",
                "[" + DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss+00:00") + "]",
                logLevel.ToString(),
                formatter(state, exception),
                exception != null ? exception.StackTrace : "");

            Directory.CreateDirectory(_loggerFileProvider.Options.FolderPath);
            #pragma warning restore SCS0031

            await _semaphore.WaitAsync();
            try
            {
                using (var streamWriter = new StreamWriter(fullFilePath, true))
                {
                    await streamWriter.WriteLineAsync(logRecord);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logging to file failed: {ex.Message}");
            }
            finally
            {
                _semaphore.Release(); // Release the semaphore
            }
        }

        private string BuildFilePath()
        {
            return Path.Combine(
                _loggerFileProvider.Options.FolderPath,
                _loggerFileProvider.Options.FilePath.Replace("{date}", DateTime.UtcNow.ToString("yyyyMMdd"))
            );
        }
    }
}