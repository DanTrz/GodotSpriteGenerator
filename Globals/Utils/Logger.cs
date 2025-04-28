using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public static class Log
{
    //Defines additional debug log info components to be displayed
    static bool _showTimeStamp = false;
    static bool _showDeclaringClass = false;
    static bool _showCallingMethod = false;

    //Individually select active types of debug messages
    static bool _debugMessages = false;
    static bool _infoMessages = false;
    static bool _warningMessages = false;
    static bool _errorMessages = false;

    //Overrides the individual selection of debug messages
    static bool _enableAllMessages = true;
    static bool _disableAllMessages = false;

    static bool _isVisualStudioDebugger = false;

    private static ConcurrentQueue<LogEntry> _logQueue = new ConcurrentQueue<LogEntry>();
    private static Task _processingTask;
    private static readonly object _processingLock = new object();

    private class LogEntry
    {
        public Node CallerNode { get; set; }
        public LogLevel Level { get; set; }
        public object[] Message { get; set; }
    }

    public enum LogLevel
    {
        ERROR,
        WARNING,
        DEBUG,
        INFO
    }

    static Log()
    {
        if (!OS.IsDebugBuild())
        {
            _debugMessages = false;
            _infoMessages = false;
            _warningMessages = false;
            _errorMessages = true;
        }

        _isVisualStudioDebugger = IsRunningInVisualStudio();
        StartProcessingQueueAsync();
    }

    private static bool IsRunningInVisualStudio()
    {
        string vsVersion = System.Environment.GetEnvironmentVariable("VisualStudioVersion");
        return !string.IsNullOrEmpty(vsVersion) && vsVersion.StartsWith("17.0", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task ProcessLogQueueAsync()
    {
        while (true)
        {
            if (_logQueue.TryDequeue(out var entry))
            {
                await AddInternalAsync(entry.CallerNode, entry.Level, entry.Message);
            }
            else
            {
                await Task.Delay(1); // Small delay to avoid busy-waiting
            }
        }
    }

    private static void StartProcessingQueueAsync()
    {
        lock (_processingLock)
        {
            if (_processingTask == null || _processingTask.IsCompleted)
            {
                _processingTask = Task.Run(ProcessLogQueueAsync);
            }
        }
    }

    private static async Task AddInternalAsync(Node callerNode, LogLevel level, params object[] message)
    {
        if (_disableAllMessages) return;

        string timeStamp = _showTimeStamp ? $"[{DateTime.Now:yy-MM-dd HH:mm:ss}]" : "";
        string callingMethod = "";
        string declaringClass = "";

        try
        {
            if (_showDeclaringClass || _showCallingMethod)
            {
                var frame = new System.Diagnostics.StackTrace().GetFrame(2);
                if (frame != null)
                {
                    if (_showDeclaringClass)
                    {
                        var className = frame.GetMethod()?.DeclaringType?.Name;
                        declaringClass = $"[{(string.IsNullOrEmpty(className) ? "ClassNameUnknown" : className)}{(callerNode != null ? $"(Node:{callerNode.Name})" : "")}]";
                    }

                    if (_showCallingMethod)
                    {
                        var method = frame.GetMethod()?.Name;
                        callingMethod = $"[{(string.IsNullOrEmpty(method) ? "MethodUnknown" : method)}]";
                    }
                }
            }
        }
        catch (Exception error)
        {
            GD.PrintErr($"Error in Logger {error.Message}");
        }

        string logMessage = $"{timeStamp}[{level}]{declaringClass}{callingMethod} ";
        string color = level switch { LogLevel.DEBUG => "WHITE", LogLevel.INFO => "CYAN", LogLevel.WARNING => "YELLOW", LogLevel.ERROR => "RED", _ => "CYAN" };

        if (!_isVisualStudioDebugger)
        {
            GD.PrintRich([$"[color={color}]{logMessage}[/color]", .. message]);
        }
        else
        {
            Debugger.Log((int)level, "Messages", logMessage + AppendPrintParams(message) + "\r\n");
        }

        await Task.Yield(); // To ensure it's an async method
    }

    public static void Debug(params object[] message)
    {
        if (_debugMessages || _enableAllMessages)
            _logQueue.Enqueue(new LogEntry { Level = LogLevel.DEBUG, Message = message });
    }

    public static void Debug(Node callerNode, params object[] message)
    {
        if (_debugMessages || _enableAllMessages)
            _logQueue.Enqueue(new LogEntry { CallerNode = callerNode, Level = LogLevel.DEBUG, Message = message });
    }

    public static void Info(params object[] message)
    {
        if (_infoMessages || _enableAllMessages)
            _logQueue.Enqueue(new LogEntry { Level = LogLevel.INFO, Message = message });
    }

    public static void Info(Node callerNode, params object[] message)
    {
        if (_infoMessages || _enableAllMessages)
            _logQueue.Enqueue(new LogEntry { CallerNode = callerNode, Level = LogLevel.INFO, Message = message });
    }

    public static void Warning(params object[] message)
    {
        if (_warningMessages || _enableAllMessages)
            _logQueue.Enqueue(new LogEntry { Level = LogLevel.WARNING, Message = message });
    }

    public static void Warning(Node callerNode, params object[] message)
    {
        if (_warningMessages || _enableAllMessages)
            _logQueue.Enqueue(new LogEntry { CallerNode = callerNode, Level = LogLevel.WARNING, Message = message });
    }

    public static void Error(params object[] message)
    {
        if (_errorMessages || _enableAllMessages)
            _logQueue.Enqueue(new LogEntry { Level = LogLevel.ERROR, Message = message });
    }

    public static void Error(Node callerNode, params object[] message)
    {
        if (_errorMessages || _enableAllMessages)
            _logQueue.Enqueue(new LogEntry { CallerNode = callerNode, Level = LogLevel.ERROR, Message = message });
    }

    private static string AppendPrintParams(object[] parameters)
    {
        if (parameters == null || parameters.Length == 0)
            return "null";
        return string.Join(string.Empty, parameters.Select(p => p?.ToString() ?? "null"));
    }
}
