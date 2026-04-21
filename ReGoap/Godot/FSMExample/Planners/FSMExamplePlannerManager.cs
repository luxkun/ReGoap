using ReGoap.Godot;
using ReGoap.Planner;

namespace ReGoap.Godot.FSMExample.Planners
{
    public partial class FSMExamplePlannerManager : ReGoapPlannerManager<string, object>
    {
        public override void _Ready()
        {
            PlannerSettings = new ReGoapPlannerSettings
            {
                UseWeightedRandomGoalSelection = true,
                WeightedRandomGoalPriorityPower = 1f,
                WeightedRandomMinimumWeight = 0.001f,
                WeightedRandomUseDeterministicSeed = false,
            };

            base._Ready();
        }
    }
}
