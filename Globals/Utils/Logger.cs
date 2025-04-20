using System;
using Godot;

public static class Log
{
    static bool DebugMessages = true;
    static bool InfoMessages = true;
    static bool WarningMessages = true;
    static bool ErrorMessages = true;
    static bool ShowTimeStamp = false;
    static bool ShowDeclaringClass = true;
    static bool ShowCallingMethod = false;

    public enum LogLevel
    {
        DEBUG,
        INFO,
        WARNING,
        ERROR,
    }

    static Log()
    {
        if (!OS.IsDebugBuild())
        {
            DebugMessages = false;
            InfoMessages = false;
            WarningMessages = false;
            ErrorMessages = true;
        }
    }

    public static void Add(Node callerNode = null, LogLevel level = LogLevel.DEBUG, params object[] message)
    {
        string timeStamp = "";
        string callingMethod = "";
        string declaringClass = "";

        try
        {
            if (ShowTimeStamp)
            {
                var dateTime = DateTime.Now;
                timeStamp = $"[{dateTime:yy-MM-dd HH:mm:ss}]";
            }

            if (ShowDeclaringClass)
            {
                var className = new System.Diagnostics.StackTrace().GetFrame(2).GetMethod().DeclaringType.Name.ToString();

                if (className == null || className == "")
                {
                    className = "ClassNameUnknown";
                }

                declaringClass = $"[{className}]";

                if (callerNode != null)
                {
                    declaringClass = $"[{declaringClass}(Node:{callerNode.Name})]";
                }


            }

            if (ShowCallingMethod)
            {
                var method = new System.Diagnostics.StackTrace().GetFrame(2).GetMethod().Name.ToString();

                if (method == null || method == "")
                {
                    method = "MethodUnknown";
                }

                callingMethod = $"[{method}]";
            }

        }
        catch (System.Exception error)
        {
            GD.PrintErr($"Error in Logger {error.Message}");
        }

        string logMessage = $"{timeStamp}[{level}]{declaringClass}{callingMethod} ";
        string color = "CYAN";

        switch (level)
        {
            case LogLevel.DEBUG:
                color = "WHITE";
                break;
            case LogLevel.INFO:
                color = "CYAN";
                break;
            case LogLevel.WARNING:
                color = "YELLOW";
                break;
            case LogLevel.ERROR:
                color = "RED";
                break;
            default:
                break;
        }

        GD.PrintRich([$"[color={color}]{logMessage}[/color]", .. message]);
    }

    public static void Debug(params object[] message)
    {
        if (DebugMessages)
            Add(null, LogLevel.DEBUG, message);
    }

    public static void Debug(Node callerNode, params object[] message)
    {
        if (DebugMessages)
            Add(callerNode, LogLevel.DEBUG, message);
    }

    public static void Info(params object[] message)
    {
        if (InfoMessages)
            Add(null, LogLevel.INFO, message);
    }

    public static void Info(Node callerNode, params object[] message)
    {
        if (InfoMessages)
            Add(callerNode, LogLevel.INFO, message);
    }

    public static void Warning(params object[] message)
    {
        if (WarningMessages)
            Add(null, LogLevel.WARNING, message);
    }

    public static void Warning(Node callerNode, params object[] message)
    {
        if (WarningMessages)
            Add(callerNode, LogLevel.WARNING, message);
    }

    public static void Error(params object[] message)
    {
        if (ErrorMessages)
            Add(null, LogLevel.ERROR, message);
    }

    public static void Error(Node callerNode, params object[] message)
    {
        if (ErrorMessages)
            Add(callerNode, LogLevel.ERROR, message);
    }

}
