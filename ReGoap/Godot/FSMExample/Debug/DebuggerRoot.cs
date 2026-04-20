using ReGoap.Godot;
namespace ReGoap.Godot.FSMExample.Debug
{
    public partial class DebuggerRoot : ReGoapDebugger
    {
        public override void _Ready()
        {
            StartVisible = false;
            FollowFirstAgentWhenUnlocked = false;
            UseSeparateWindow = !ReGoapLaunchContext.IsEditorHostedRun();
            UiFontSize = 26;
            SeparateWindowSize = new global::Godot.Vector2I(1900, 1180);
            base._Ready();
        }
    }
}
