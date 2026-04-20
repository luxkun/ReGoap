using Godot;

namespace ReGoap.Godot.FSMExample.World
{
    public partial class WorldBootstrap : Node
    {
        public override void _Ready()
        {
            CallDeferred(nameof(InitializeAllWorkers));
        }

        private void InitializeAllWorkers()
        {
            var root = GetTree().Root.GetNodeOrNull<Node>("root");
            if (root == null)
            {
                GD.PushError("[WorldBootstrap] Could not find /root node.");
                return;
            }

            var workers = root.GetNodeOrNull<Node>("Workers");
            if (workers == null)
            {
                GD.PushError("[WorldBootstrap] Could not find Workers container.");
                return;
            }

            foreach (var childObj in workers.GetChildren())
            {
                if (childObj is WorkerAgent worker)
                {
                    worker.RefreshMemory();
                    worker.RefreshGoalsSet();
                    worker.RefreshActionsSet();
                    worker.ForceReplanNow();
                }
            }
        }
    }
}
