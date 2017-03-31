#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


public class ReGoapLogger
{
#if UNITY_5_3_OR_NEWER
    private class UnityTraceListener : IListener
    {
        public void Write(string message)
        {
            Write(message, "");
        }

        public void Write(string message, string category)
        {
            switch (category)
            {
                case "error":
                    UnityEngine.Debug.LogError(message);
                    break;
                case "warning":
                    UnityEngine.Debug.LogWarning(message);
                    break;
                default:
                    UnityEngine.Debug.Log(message);
                    break;
            }
        }
    }
#else 
    private class GenericTraceListener : IListener
    {
        public void Write(string message)
        {
            Write(message, "");
        }

        public void Write(string message, string category)
        {
            Console.WriteLine(message);
        }
    }
#endif

    [Flags]
    public enum DebugLevel
    {
        None, ErrorsOnly, WarningsOnly, Full
    }
    public DebugLevel Level = DebugLevel.Full;
    public bool RunOnlyOnMainThread = true;

    private static readonly ReGoapLogger instance = new ReGoapLogger();
    public static ReGoapLogger Instance
    {
        get { return instance; }
    }

    private readonly IListener listener;

    private readonly int mainThreadId;

    private ReGoapLogger()
    {
        mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

#if UNITY_5_3_OR_NEWER
        listener = new UnityTraceListener();
#else
        listener = new GenericTraceListener();
#endif
    }

    private static bool InMainThread()
    {
        return !instance.RunOnlyOnMainThread || System.Threading.Thread.CurrentThread.ManagedThreadId == instance.mainThreadId;
    }

    public static void Log(string message)
    {
        if (Instance.Level != DebugLevel.Full || !InMainThread()) return;
        instance.listener.Write(message);
    }

    public static void LogWarning(string message)
    {
        if (Instance.Level < DebugLevel.WarningsOnly || !InMainThread()) return;
        instance.listener.Write(message, "warning");
    }

    public static void LogError(string message)
    {
        if (Instance.Level < DebugLevel.ErrorsOnly || !InMainThread()) return;
        instance.listener.Write(message, "error");
    }
}

internal interface IListener
{
    void Write(string text);
    void Write(string text, string category);
}