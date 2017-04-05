namespace ReGoap.Unity
{
    public class ReGoapAgentAdvanced<T, W> : ReGoapAgent<T, W>
    {
        public bool ValidateActiveAction;

        #region UnityFunctions
        protected virtual void Update()
        {
            possibleGoalsDirty = true;

            if (currentActionState == null)
            {
                if (!IsPlanning)
                    CalculateNewGoal();
                return;
            }
            // check if current action preconditions are still valid, else invalid action and restart planning
            if (ValidateActiveAction)
            {
                var state = memory.GetWorldState();
                if (currentActionState.Action.GetPreconditions(state).MissingDifference(state, 1) > 0)
                    TryWarnActionFailure(currentActionState.Action);
            }
        }
        #endregion
    }
}