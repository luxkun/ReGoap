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
            switch (category)
            {
                case "error":
                    System.Diagnostics.Debug.WriteLine(message);
                    break;
                case "warning":
                    System.Diagnostics.Debug.WriteLine(message);
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine(message);
                    break;
            }
        }
    }
#endif

    public bool enabled;

    private static readonly ReGoapLogger instance = new ReGoapLogger();
    public static ReGoapLogger Instance
    {
        get { return instance; }
    }

    private readonly IListener listener;

    private ReGoapLogger()
    {
#if UNITY_5_3_OR_NEWER
        listener = new UnityTraceListener();
#else
        listener = new GenericTraceListener();
#endif
        enabled = true;
    }

    public static void Log(string message)
    {
        if (!instance.enabled) return;
        instance.listener.Write(message);
    }

    public static void LogWarning(string message)
    {
        if (!instance.enabled) return;
        instance.listener.Write(message, "warning");
    }

    public static void LogError(string message)
    {
        if (!instance.enabled) return;
        instance.listener.Write(message, "error");
    }
}

internal interface IListener
{
    void Write(string text);
    void Write(string text, string category);
}