using System;
using System.Collections.Generic;
using ReGoap.Planner;

namespace ReGoap.Core
{
    /// <summary>
    /// Contract implemented by GOAP goals.
    /// </summary>
    public interface IReGoapGoal<T, W>
    {
        /// <summary>
        /// Starts goal runtime behavior.
        /// </summary>
        void Run(Action<IReGoapGoal<T, W>> callback);

        /// <summary>
        /// Returns current plan queue associated with this goal.
        /// Must be thread-safe.
        /// </summary>
        Queue<ReGoapActionState<T, W>> GetPlan();

        /// <summary>
        /// Returns goal display name.
        /// </summary>
        string GetName();

        /// <summary>
        /// Planner-time precomputation hook.
        /// </summary>
        void Precalculations(IGoapPlanner<T, W> goapPlanner);

        /// <summary>
        /// Returns whether this goal is currently eligible.
        /// </summary>
        bool IsGoalPossible();

        /// <summary>
        /// Returns desired goal state.
        /// </summary>
        ReGoapState<T, W> GetGoalState();

        /// <summary>
        /// Goal priority used for goal ordering.
        /// </summary>
        float GetPriority();

        /// <summary>
        /// Stores planner result for this goal.
        /// </summary>
        void SetPlan(Queue<ReGoapActionState<T, W>> path);

        /// <summary>
        /// Delay before retrying this goal after a planning/execution error.
        /// </summary>
        float GetErrorDelay();
    }
}
