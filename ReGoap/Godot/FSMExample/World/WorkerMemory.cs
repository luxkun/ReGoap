using ReGoap.Godot;

namespace ReGoap.Godot.FSMExample.World
{
    public partial class WorkerMemory : ReGoapMemoryAdvanced<string, object>
    {
        public override void _Ready()
        {
            base._Ready();
            var world = GetWorldState();
            world.Set("hasWood", false);
            world.Set("hasOre", false);
            world.Set("hasIngot", false);
            world.Set("randomMoveCount", 0);
        }
    }
}
