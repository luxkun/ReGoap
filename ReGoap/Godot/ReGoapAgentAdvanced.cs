namespace ReGoap.Godot
{
    public partial class ReGoapAgentAdvanced<T, W> : ReGoapAgent<T, W>
    {
        public override void _Process(double delta)
        {
            possibleGoalsDirty = true;

            if (currentActionState == null)
            {
                if (!IsPlanning)
                    CalculateNewGoal();
                return;
            }
        }
    }
}
