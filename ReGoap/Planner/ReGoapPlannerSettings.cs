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

        /// <summary>
        /// If true, goal selection uses weighted-random sampling based on priority,
        /// instead of always trying the highest-priority goal first.
        /// </summary>
        public bool UseWeightedRandomGoalSelection = false;

        /// <summary>
        /// Exponent applied to priority when weighted-random goal selection is enabled.
        /// 1 = linear weighting, values greater than 1 bias more strongly toward high-priority goals.
        /// </summary>
        public float WeightedRandomGoalPriorityPower = 1f;

        /// <summary>
        /// Minimum per-goal weight used when weighted-random selection is enabled.
        /// Keeps very low/zero priorities selectable with a small probability.
        /// </summary>
        public float WeightedRandomMinimumWeight = 0.001f;

        /// <summary>
        /// Enables deterministic weighted-random goal selection for tests/replays.
        /// </summary>
        public bool WeightedRandomUseDeterministicSeed = false;

        /// <summary>
        /// Seed used when deterministic weighted-random selection is enabled.
        /// </summary>
        public int WeightedRandomSeed = 0;
    }
}
