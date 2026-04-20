using System;

namespace ReGoap.Godot
{
    public static class ReGoapLaunchContext
    {
        private static bool HasEditorPidArg()
        {
            foreach (var arg in global::Godot.OS.GetCmdlineArgs())
            {
                if (arg.StartsWith("--editor-pid", StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static bool IsEmbeddedDisplayServer()
        {
            return string.Equals(global::Godot.DisplayServer.GetName(), "embedded", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsLaunchedDirectly()
        {
            return !IsEditorHostedRun();
        }

        public static bool IsEditorHostedRun()
        {
            return global::Godot.Engine.IsEditorHint() ||
                   HasEditorPidArg() ||
                   IsEmbeddedDisplayServer();
        }
    }
}
