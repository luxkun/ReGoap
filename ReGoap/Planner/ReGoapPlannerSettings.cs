using System;

namespace ReGoap.Planner
{
    [Serializable]
    /// <summary>
    /// Planner configuration limits and behavior flags.
    /// </summary>
    public class ReGoapPlannerSettings
    {
        public bool PlanningEarlyExit = false;
        /// <summary>
        /// Maximum A* iterations per planning attempt.
        /// </summary>
        public int MaxIterations = 1000;

        /// <summary>
        /// Maximum expanded nodes for internal A* queue.
        /// </summary>
        public int MaxNodesToExpand = 10000;

        /// <summary>
        /// Enables planner mode tailored for dynamic actions
        /// whose preconditions/effects change at runtime.
        /// </summary>
        public bool UsingDynamicActions = false;

        /// <summary>
        /// Enables extra plan-debug information in logs.
        /// </summary>
        public bool DebugPlan = false;
    }
}
