using ReGoap.Core;

namespace ReGoap.Unity.Test
{
    public class ReGoapTestGoal : ReGoapGoal<string, object>
    {
        public void Init()
        {
            Awake();
        }

        public void SetGoalState(ReGoapState<string, object> goalState)
        {
            goal = goalState;
        }

        public void SetPriority(int priority)
        {
            this.Priority = priority;
        }
    }
}