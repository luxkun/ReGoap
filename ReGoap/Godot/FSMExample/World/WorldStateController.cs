using Godot;

namespace ReGoap.Godot.FSMExample.World
{
    public partial class WorldStateController : Node
    {
        public ChestNode Chest { get; private set; }
        public ResourceNode TreeA { get; private set; }
        public ResourceNode TreeB { get; private set; }
        public ResourceNode MineA { get; private set; }
        public ResourceNode MineB { get; private set; }
        public BuildingNode Smelter { get; private set; }
        public BuildingNode Blacksmith { get; private set; }

        public override void _Ready()
        {
            Chest = GetNode<ChestNode>("../Chest");
            TreeA = GetNode<ResourceNode>("../TreeA");
            TreeB = GetNode<ResourceNode>("../TreeB");
            MineA = GetNode<ResourceNode>("../MineA");
            MineB = GetNode<ResourceNode>("../MineB");
            Smelter = GetNode<BuildingNode>("../Smelter");
            Blacksmith = GetNode<BuildingNode>("../Blacksmith");
        }

        public ResourceNode FindBestTree()
        {
            if (TreeA.Charges >= TreeB.Charges)
                return TreeA;
            return TreeB;
        }

        public ResourceNode FindBestMine()
        {
            if (MineA.Charges >= MineB.Charges)
                return MineA;
            return MineB;
        }
    }
}
