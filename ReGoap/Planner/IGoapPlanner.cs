using System;
using System.Collections.Generic;
using ReGoap.Core;

namespace ReGoap.Planner
{
    /// <summary>
    /// Planner contract used by goals/actions during planning.
    /// </summary>
    public interface IGoapPlanner<T, W>
    {
        /// <summary>
        /// Runs planning for an agent and returns selected goal (if any).
        /// </summary>
        IReGoapGoal<T, W> Plan(IReGoapAgent<T, W> goapAgent, IReGoapGoal<T, W> blacklistGoal, Queue<ReGoapActionState<T, W>> currentPlan, Action<IReGoapGoal<T, W>> callback);

        /// <summary>
        /// Returns current goal under evaluation.
        /// </summary>
        IReGoapGoal<T, W> GetCurrentGoal();

        /// <summary>
        /// Returns current agent under evaluation.
        /// </summary>
        IReGoapAgent<T, W> GetCurrentAgent();

        /// <summary>
        /// Returns true while planner is actively calculating.
        /// </summary>
        bool IsPlanning();

        /// <summary>
        /// Returns planner tuning settings.
        /// </summary>
        ReGoapPlannerSettings GetSettings();
    }
}
