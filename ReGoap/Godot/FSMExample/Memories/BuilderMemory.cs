using ReGoap.Godot;

namespace ReGoap.Godot.FSMExample.Memories
{
    public partial class BuilderMemory : ReGoapMemoryAdvanced<string, object>
    {
        public override void _Ready()
        {
            base._Ready();
            var worldState = GetWorldState();
            worldState.Set("hasTool", true);
        }
    }
}
