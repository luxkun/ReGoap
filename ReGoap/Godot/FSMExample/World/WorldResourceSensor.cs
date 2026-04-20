using ReGoap.Godot;

namespace ReGoap.Godot.FSMExample.World
{
    public partial class WorldResourceSensor : ReGoapSensor<string, object>
    {
        public override void UpdateSensor()
        {
            var worldState = memory.GetWorldState();
            var bindings = GetNodeOrNull<WorkerAgentBindings>("../../Bindings");
            if (bindings == null || bindings.World == null || bindings.World.Chest == null)
                return;

            var chest = bindings.World.Chest;

            worldState.Set("chestWoodCount", chest.Wood);
            worldState.Set("chestOreCount", chest.IronOre);
            worldState.Set("chestIngotCount", chest.IronIngot);
            worldState.Set("chestSwordCount", chest.Swords);
        }
    }
}
