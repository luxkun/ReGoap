namespace ReGoap.Godot
{
    public partial class ReGoapGoalAdvanced<T, W> : ReGoapGoal<T, W>
    {
        public float WarnDelay = 2f;
        private float warnCooldown;

        public override void _Process(double delta)
        {
            if (planner != null && !planner.IsPlanning() && GetTime() > warnCooldown)
            {
                warnCooldown = GetTime() + WarnDelay;
                var currentGoal = planner.GetCurrentGoal();
                var plannerPlan = currentGoal == null ? null : currentGoal.GetPlan();
                var equalsPlan = ReferenceEquals(plannerPlan, plan);
                var isGoalPossible = IsGoalPossible();
                if ((!equalsPlan && isGoalPossible) || (equalsPlan && !isGoalPossible))
                    planner.GetCurrentAgent().WarnPossibleGoal(this);
            }
        }

        protected virtual float GetTime()
        {
            return (float)(System.DateTime.UtcNow.Ticks / (double)System.TimeSpan.TicksPerSecond);
        }
    }
}
