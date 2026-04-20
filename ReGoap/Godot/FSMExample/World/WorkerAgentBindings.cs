using Godot;

namespace ReGoap.Godot.FSMExample.World
{
    public partial class WorkerAgentBindings : Node
    {
        public WorkerPawn Pawn { get; private set; }
        public WorldStateController World { get; private set; }

        public override void _Ready()
        {
            Pawn = GetNodeOrNull<WorkerPawn>("../Pawn");
            if (Pawn == null)
                Pawn = GetParent()?.GetNodeOrNull<WorkerPawn>("Pawn");

            var cursor = GetParent();
            while (cursor != null && cursor.Name != "root")
                cursor = cursor.GetParent();

            if (cursor != null)
                World = cursor.GetNodeOrNull<WorldStateController>("World");

            if (World == null)
                World = GetTree().Root.GetNodeOrNull<WorldStateController>("root/World");

            if (Pawn == null || World == null)
            {
                GD.PushError("[WorkerAgentBindings] Failed to resolve Pawn/World at " + GetPath());
            }
        }
    }
}
