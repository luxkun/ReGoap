#define VERBOSE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Debug = System.Diagnostics.Debug;


public class ReGoapLogger
{
#if UNITY_5_3_OR_NEWER
    private class UnityTraceListener : TraceListener
    {
        public override void Write(string message)
        {
            Write(message, "");
        }

        public override void Write(string message, string category)
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

        public override void WriteLine(string message)
        {
            Write(message);
        }

        public override void WriteLine(string message, string category)
        {
            Write(message, category);
        }
    }
#endif

    private ReGoapLogger()
    {
#if UNITY_5_3_OR_NEWER
        Trace.Listeners.Add(new UnityTraceListener());
#else
        Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
#endif
        Trace.AutoFlush = true;
    }

    public static void Log(string message)
    {
        Trace.WriteLine(message);
    }

    public static void LogWarning(string message)
    {
        Trace.WriteLine(message, "warning");
    }

    public static void LogError(string message)
    {
        Trace.WriteLine(message, "error");
    }
}