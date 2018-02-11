using ReGoap.Core;

namespace ReGoap.Unity
{
    public class ReGoapAgentAdvanced<T, W> : ReGoapAgent<T, W>
    {
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
        }
        #endregion
    }
}