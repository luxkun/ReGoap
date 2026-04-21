using ReGoap.Core;
using ReGoap.Godot;

namespace ReGoap.Godot.FSMExample.World
{
    /// <summary>
    /// Low-priority ambient goal that asks the worker to complete one wander cycle.
    /// </summary>
    public partial class RandomMoveGoal : ReGoapGoal<string, object>
    {
        private int targetMoveCount;

        public override void _Ready()
        {
            base._Ready();
            Name = "Random Move Around";
            Priority = 2.5f;
            targetMoveCount = -1;
        }

        public override void Precalculations(ReGoap.Planner.IGoapPlanner<string, object> goapPlanner)
        {
            base.Precalculations(goapPlanner);

            var state = goapPlanner.GetCurrentAgent().GetMemory().GetWorldState();
            var moveCountObj = state.Get("randomMoveCount");
            var moveCount = moveCountObj is int value ? value : 0;

            if (targetMoveCount <= moveCount)
                targetMoveCount = moveCount + 1;

            goal.Clear();
            goal.Set("randomMoveCount", ReGoapCondition.GreaterOrEqual(targetMoveCount));
        }
    }
}
