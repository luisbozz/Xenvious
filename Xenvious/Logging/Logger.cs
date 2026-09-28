using System;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Threading;
using System.Threading.Tasks;

namespace Xenvious.Logging
{
    /// <summary>
    /// Static entrypoint for convenient, IntelliSense-friendly logging calls.
    /// Toggle file-writing via <see cref="FileLoggingEnabled"/>.
    /// </summary>
    public static class Log
    {
        /// <summary>Global logger instance used by the UI and helpers.</summary>
        public static Logger Instance { get; } = new Logger();

        /// <summary>
        /// Global toggle you can bind to a CheckBox or set programmatically.
        /// </summary>
        public static bool FileLoggingEnabled
        {
            get => Instance.FileLoggingEnabled;
            set => Instance.FileLoggingEnabled = value;
        }

        /// <summary>Directory where log files are written (daily rolling).</summary>
        public static string LogDirectory
        {
            get => Instance.LogDirectory;
            set => Instance.LogDirectory = value;
        }

        /// <summary>Gibt an, ab welchem Level *UI*-Logs erzeugt werden (Trace < Debug < Info < Warn < Error < Fatal).</summary>
        public static LogLevel MinimumLevel
        {
            get => Instance.MinimumLevel;
            set => Instance.MinimumLevel = value;
        }

        /// <summary>Gibt an, ab welchem Level *Datei*-Logs geschrieben werden.</summary>
        public static LogLevel MinimumLevelFile
        {
            get => Instance.MinimumLevelFile;
            set => Instance.MinimumLevelFile = value;
        }

        // Convenience helpers
        public static void Trace(string message, Exception ex = null, string source = null) => Instance.Log(LogLevel.Trace, message, ex, source);
        public static void Debug(string message, Exception ex = null, string source = null) => Instance.Log(LogLevel.Debug, message, ex, source);
        public static void Info(string message, Exception ex = null, string source = null)  => Instance.Log(LogLevel.Info,  message, ex, source);
        public static void Warn(string message, Exception ex = null, string source = null)  => Instance.Log(LogLevel.Warn,  message, ex, source);
        public static void Error(string message, Exception ex = null, string source = null) => Instance.Log(LogLevel.Error, message, ex, source);
        public static void Fatal(string message, Exception ex = null, string source = null) => Instance.Log(LogLevel.Fatal, message, ex, source);
        public static void Write(LogLevel level, string message, Exception ex = null, string source = null) => Instance.Log(level, message, ex, source);
    }

    public enum LogLevel
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }

    public sealed class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Source { get; set; }
        public Exception Exception { get; set; }

        /// <summary>
        /// Convenience: sichere, formatierte Exception-Nachricht (z.B. "JsonException: ...").
        /// Gibt null zurück, wenn keine Exception vorhanden ist.
        /// </summary>
        public string ExceptionMessage
        {
            get
            {
                if (Exception == null) return null;
                try { return $"{Exception.GetType().Name}: {Exception.Message}"; }
                catch { return Exception.Message; }
            }
        }

        public override string ToString()
        {
            var src = string.IsNullOrWhiteSpace(Source) ? string.Empty : $" [{Source}]";
            var ex  = Exception == null ? string.Empty : $" | EX: {Exception.GetType().Name}: {Exception.Message}" + InnerChain(Exception);
            return $"[{Timestamp:HH:mm:ss.fff}] {Level.ToString().ToUpper(),-5}{src} {Message}{ex}";
        }

        // Wrappers like TargetInvocationException or XamlParseException say little on their own;
        // the cause is further in, together with where it was thrown.
        private static string InnerChain(Exception exception)
        {
            var text = new System.Text.StringBuilder();
            for (var inner = exception.InnerException; inner != null; inner = inner.InnerException)
            {
                text.Append($" <- {inner.GetType().Name}: {inner.Message}");
                string frame = inner.StackTrace?.Split('\n').FirstOrDefault()?.Trim();
                if (!string.IsNullOrEmpty(frame))
                    text.Append($" ({frame})");
            }
            return text.ToString();
        }
    }

    /// <summary>
    /// The core logger that raises events for the UI and (optionally) writes to a rolling file.
    /// </summary>
    public sealed class Logger
    {
        private readonly SemaphoreSlim _fileLock = new(1, 1);

        /// <summary>Raised whenever a new <see cref="LogEntry"/> is created.</summary>
        public event EventHandler<LogEntry> EntryAdded;

        /// <summary>Max number of entries the UI should keep in memory.</summary>
        public int MaxEntries { get; set; } = 5000;

        /// <summary>
        /// Toggle to write log lines to file. Bind this to your CheckBox.
        /// </summary>
        public bool FileLoggingEnabled { get; set; } = false;

        /// <summary>Ab welchem Level Einträge an die UI durchgereicht werden.</summary>
        public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

        /// <summary>Ab welchem Level in die Datei geschrieben wird.</summary>
        public LogLevel MinimumLevelFile { get; set; } = LogLevel.Debug;

        /// <summary>
        /// Directory where logs are written. Defaults to %APPDATA%\Xenvious\ (absolute path).
        /// </summary>
        public string LogDirectory { get; set; } = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Xenvious"));

        /// <summary>Current log file path (one file per day).</summary>
        public string CurrentLogFilePath => Path.Combine(LogDirectory, $"{DateTime.Now:yyyy-MM-dd}.log");

        public Logger()
        {
            Directory.CreateDirectory(LogDirectory);
        }

        /// <summary>
        /// Create a new log entry and notify subscribers. Safe to call from any thread.
        /// </summary>
        public void Log(LogLevel level, string message, Exception ex = null, string source = null)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                Exception = ex,
                Source = source
            };

            // Fire-and-forget to UI and file
            if (level >= MinimumLevel)
            {
                if (EntryAdded != null) EntryAdded.Invoke(this, entry);
            }
            if (FileLoggingEnabled && level >= MinimumLevelFile)
            {
                _ = WriteToFileAsync(entry);
            }
        }

        private async Task WriteToFileAsync(LogEntry entry)
        {
            var line = entry.ToString();
            try
            {
                await _fileLock.WaitAsync().ConfigureAwait(false);
                Directory.CreateDirectory(LogDirectory);
                await Task.Run(() => File.AppendAllText(CurrentLogFilePath, line + Environment.NewLine)).ConfigureAwait(false);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        // Instance-level convenience methods (optional)
        public void Trace(string message, Exception ex = null, string source = null) => Log(LogLevel.Trace, message, ex, source);
        public void Debug(string message, Exception ex = null, string source = null) => Log(LogLevel.Debug, message, ex, source);
        public void Info (string message, Exception ex = null, string source = null) => Log(LogLevel.Info , message, ex, source);
        public void Warn (string message, Exception ex = null, string source = null) => Log(LogLevel.Warn , message, ex, source);
        public void Error(string message, Exception ex = null, string source = null) => Log(LogLevel.Error, message, ex, source);
        public void Fatal(string message, Exception ex = null, string source = null) => Log(LogLevel.Fatal, message, ex, source);
    }

}