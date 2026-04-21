using ReGoap.Core;
using ReGoap.Godot;

namespace ReGoap.Godot.FSMExample.World
{
    public partial class SwordsmithGoal : ReGoapGoal<string, object>
    {
        private int targetSwordCount;

        public override void _Ready()
        {
            base._Ready();
            Name = "Craft Sword";
            Priority = 10.0f;
            targetSwordCount = -1;
        }

        public override void Precalculations(ReGoap.Planner.IGoapPlanner<string, object> goapPlanner)
        {
            base.Precalculations(goapPlanner);

            var state = goapPlanner.GetCurrentAgent().GetMemory().GetWorldState();
            var swordsInChestObj = state.Get("chestSwordCount");
            var swordsInChest = swordsInChestObj is int value ? value : 0;

            if (targetSwordCount <= swordsInChest)
                targetSwordCount = swordsInChest + 1;

            goal.Clear();
            goal.Set("chestSwordCount", ReGoapCondition.GreaterOrEqual(targetSwordCount));
        }
    }
}
