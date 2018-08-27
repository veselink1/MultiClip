using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MultiClip.Utilities
{
    /// <summary>
    /// Specifies the severity of an event.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// A tracing log. Do not use in release mode.
        /// </summary>
        Trace = 0,
        /// <summary>
        /// A debug lo. Do not use in release mode.
        /// </summary>
        Debug = 1,
        /// <summary>
        /// An information log.
        /// </summary>
        Information = 2,
        /// <summary>
        /// A warning log, meaning that a runtime 
        /// error was handled successfully.
        /// </summary>
        Warning = 3,
        /// <summary>
        /// An error log, meaning that a runtime
        /// error was not handled, but is not critical.
        /// </summary>
        Error = 4,
        /// <summary>
        /// A critical log, meaning that a critical runtime 
        /// error was not handled, and the application was closed forcefully.
        /// </summary>
        Critical = 5,
    }

    /// <summary>
    /// Represents a unique event identifier, containing the 
    /// developer-oriented code and name of the event.
    /// </summary>
    public sealed class EventId
    {
        /// <summary>
        /// The ID of the event. This value should be preserved across 
        /// different application versions.
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// The name of the event. This value should be preserved across 
        /// different application versions.
        /// </summary>
        public string Name { get; private set; }

        public EventId(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    /// <summary>
    /// Exposes an interface for logging individual events.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Checks if the gived LogLevel is enabled.
        /// </summary>
        bool IsEnabled(LogLevel logLevel);

        /// <summary>
        /// Logs the specified information.
        /// </summary>
        /// <param name="logLevel">The severity of the event.</param>
        /// <param name="eventId">The descriptor of the event.</param>
        /// <param name="message">The (optional) message for the event.</param>
        /// <param name="state">The (optional) state for the event.</param>
        void Log(LogLevel logLevel, EventId eventId, string message, object state);

        /// <summary>
        /// Returns when all pending writes are executed.
        /// </summary>
        void WaitWriter();
    }

    /// <summary>
    /// Exposes an interface that allows the appropriate handling
    /// of individual logged messages.
    /// </summary>
    public interface ILogWriter
    {
        void Write(IEnumerable<string> entries);
    }

    /// <summary>
    /// Exposes an interface that allows for the proper formatting of
    /// an event and all of its additional data.
    /// </summary>
    public interface ILogFormatter
    {
        string Format(LogLevel logLevel, EventId eventId, string message, object state);
    }

    public static class LoggerExtensions
    {
        /// <summary>
        /// Writes an event to the logger.
        /// </summary>
        public static void Log(this ILogger logger, LogLevel logLevel, EventId eventId)
        {
            logger.Log(logLevel, eventId, null, null);
        }

        /// <summary>
        /// Writes an event to the logger.
        /// </summary>
        public static void Log(this ILogger logger, LogLevel logLevel, EventId eventId, string message)
        {
            logger.Log(logLevel, eventId, message, null);
        }

        /// <summary>
        /// Writes an event to the logger.
        /// </summary>
        public static void Log(this ILogger logger, LogLevel logLevel, EventId eventId, object state)
        {
            logger.Log(logLevel, eventId, null, state);
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Information"/> event to the logger.
        /// </summary>
        public static void LogInfo(this ILogger logger, EventId eventId, string message = null, object state = null)
        {
            logger.Log(LogLevel.Information, eventId, message, state);
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Information"/> event to the logger.
        /// </summary>
        public static void LogInfo(this ILogger logger, EventId eventId, object state = null)
        {
            logger.Log(LogLevel.Information, eventId, null, state);
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Warning"/> event to the logger.
        /// </summary>
        public static void LogWarn(this ILogger logger, EventId eventId, string message = null, object state = null)
        {
            logger.Log(LogLevel.Warning, eventId, message, state);
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Warning"/> event to the logger.
        /// </summary>
        public static void LogWarn(this ILogger logger, EventId eventId, object state = null)
        {
            logger.Log(LogLevel.Warning, eventId, null, state);
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Error"/> event to the logger.
        /// </summary>
        public static void LogError(this ILogger logger, EventId eventId, string message = null, object state = null)
        {
            logger.Log(LogLevel.Error, eventId, message, state);
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Error"/> event to the logger.
        /// </summary>
        public static void LogError(this ILogger logger, EventId eventId, object state = null)
        {
            logger.Log(LogLevel.Error, eventId, null, state);
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Critical"/> event to the logger.
        /// </summary>
        public static void LogCritical(this ILogger logger, EventId eventId, string message = null, object state = null)
        {
            logger.Log(LogLevel.Critical, eventId, message, state);
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Critical"/> event to the logger.
        /// </summary>
        public static void LogCritical(this ILogger logger, EventId eventId, object state = null)
        {
            logger.Log(LogLevel.Critical, eventId, null, state);
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Debug"/> event to the logger.
        /// </summary>
        public static void LogDebug(this ILogger logger, EventId eventId, string message = null, object state = null)
        {
            logger.Log(LogLevel.Debug, eventId, message, state);
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Debug"/> event to the logger.
        /// </summary>
        public static void LogDebug(this ILogger logger, EventId eventId, object state = null)
        {
            logger.Log(LogLevel.Debug, eventId, null, state);
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Trace"/> event to the logger.
        /// </summary>
        public static void LogTrace(this ILogger logger, EventId eventId, string message = null, object state = null)
        {
            logger.Log(LogLevel.Trace, eventId, message, state);
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Trace"/> event to the logger.
        /// </summary>
        public static void LogTrace(this ILogger logger, EventId eventId, object state = null)
        {
            logger.Log(LogLevel.Trace, eventId, null, state);
        }
    }

    /// <summary>
    /// The default logger implementation, which 
    /// supports formatting and writing parameterization.
    /// </summary>
    public class Logger : ILogger
    {
        /// <summary>
        /// The default application-wide ILogger instance.
        /// Must be set by user code before usage.
        /// </summary>
        public static ILogger Default { get; set; }

        /// <summary>
        /// The current LogLevel of this ILogger.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        private readonly ILogFormatter _logFormatter;
        private readonly ILogWriter _logWriter;
        private readonly SpinLock _entryQueueLock;
        private Queue<string> _entryQueue;
        private Task _writeTask;

        /// <summary>
        /// Creates a new Logger instance, specifying 
        /// the LogLevel, ILogFormatter and ILogWriter to use.
        /// </summary>
        public Logger(LogLevel logLevel, ILogFormatter logFormatter, ILogWriter logWriter)
        {
            LogLevel = logLevel;
            _logFormatter = logFormatter;
            _logWriter = logWriter;
            _entryQueueLock = new SpinLock(false);
            _entryQueue = new Queue<string>();
            _writeTask = null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return (int)logLevel >= (int)LogLevel;
        }

        public void Log(LogLevel logLevel, EventId eventId, string message, object state)
        {
            if (IsEnabled(logLevel))
            {
                string entry = _logFormatter.Format(logLevel, eventId, message, state);
                LockQueue(() => _entryQueue.Enqueue(entry));
                StartWriteTask();
            }
        }

        public void WaitWriter()
        {
            _writeTask?.Wait();
        }

        private void LockQueue(Action action)
        {
            bool lockTaken = false;
            while (!lockTaken)
            {
                _entryQueueLock.Enter(ref lockTaken);
            }

            try
            {
                action();
            }
            finally
            {
                _entryQueueLock.Exit(useMemoryBarrier: true);
            }
        }

        private void StartWriteTask()
        {
            Task newTask = new Task(() =>
            {
                Queue<string> entries = null;
                LockQueue(() =>
                {
                    entries = _entryQueue;
                    _entryQueue = new Queue<string>();
                });
                _logWriter.Write(entries.AsEnumerable());
                _writeTask = null;
            });

            if (Interlocked.CompareExchange(ref _writeTask, newTask, null) == null)
            {
                newTask.Start();
            }
        }
    }

    /// <summary>
    /// A JSON-based ILogFormatter.
    /// </summary>
    public class JsonLogFormatter : ILogFormatter
    {
        private Formatting _formatting;

        public JsonLogFormatter(Formatting formatting)
        {
            _formatting = formatting;
        }

        public string Format(LogLevel logLevel, EventId eventId, string message, object state)
        {
            JObject obj = new JObject
            {
                ["Severity"] = logLevel.ToString("G"),
                ["DateTime"] = FormatIso8601(DateTimeOffset.Now),
                ["EventId"] = eventId.Id,
                ["EventName"] = eventId.Name
            };
            if (message != null)
            {
                obj["Message"] = message;
            }
            if (state is Exception)
            {
                obj["Exception"] = CreateExceptionToken((Exception)state);
            }
            else if (state is IEnumerable)
            {
                obj["State"] = JArray.FromObject((IEnumerable)state);
            }
            else if (state is object)
            {
                obj["State"] = JObject.FromObject(state);
            }

            return obj.ToString(_formatting);
        }

        private static string FormatIso8601(DateTimeOffset dto)
        {
            string format = dto.Offset == TimeSpan.Zero
                ? "yyyy-MM-ddTHH:mm:ss.fffZ"
                : "yyyy-MM-ddTHH:mm:ss.fffzzz";

            return dto.ToString(format, CultureInfo.InvariantCulture);
        }

        private JToken CreateExceptionToken(Exception e)
        {
            JObject obj = new JObject();
            obj["Type"] = e.GetType().FullName;
            obj["Message"] = e.Message;
#if DEBUG
            obj["StackTrace"] = e.StackTrace;
            obj["TargetSite"] = e.TargetSite != null 
                ? e.TargetSite.DeclaringType.FullName + "#" + e.TargetSite.Name 
                : null;
#endif
            if (e.HResult != 0)
            {
                obj["HResult"] = e.HResult;
            }
            if (e.InnerException != null)
            {
                obj["InnerException"] = CreateExceptionToken(e.InnerException);
            }

            return obj;
        }
    }

    /// <summary>
    /// A file-based ILogWriter.
    /// </summary>
    public class FileLogWriter : ILogWriter
    {
        public string FileName => _fileName;
        public Encoding Encoding => _encoding;

        private readonly string _fileName;
        private readonly Encoding _encoding;

        public FileLogWriter(string fileName, Encoding encoding)
        {
            _fileName = fileName;
            _encoding = encoding;
        }

        public void Write(IEnumerable<string> entries)
        {
            try
            {
                using (var fs = new FileStream(_fileName, FileMode.Append, FileAccess.Write))
                {
                    foreach (var entry in entries)
                    {
                        byte[] bytes = _encoding.GetBytes(entry + '\n');
                        fs.Write(bytes, 0, bytes.Length);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Default.LogError(new EventId(-1, "LogWriteErr"), "Failed to write some entries to the log file!", e);
            }
        }
    }
}
