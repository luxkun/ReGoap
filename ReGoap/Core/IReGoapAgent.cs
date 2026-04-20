using System.Collections.Generic;

namespace ReGoap.Core
{
    /// <summary>
    /// Contract implemented by GOAP agents.
    /// </summary>
    public interface IReGoapAgent<T, W>
    {
        /// <summary>
        /// Returns agent memory provider.
        /// </summary>
        IReGoapMemory<T, W> GetMemory();

        /// <summary>
        /// Returns currently active goal (if any).
        /// </summary>
        IReGoapGoal<T, W> GetCurrentGoal();

        /// <summary>
        /// Called by goals/systems to notify this agent of a possible higher-priority goal.
        /// </summary>
        void WarnPossibleGoal(IReGoapGoal<T, W> goal);

        /// <summary>
        /// Returns whether this agent is active in world/simulation.
        /// </summary>
        bool IsActive();

        /// <summary>
        /// Returns last accepted starting plan snapshot.
        /// </summary>
        List<ReGoapActionState<T, W>> GetStartingPlan();

        /// <summary>
        /// Gets a planner-scoped value set during plan execution.
        /// </summary>
        W GetPlanValue(T key);

        /// <summary>
        /// Sets a planner-scoped value for current plan.
        /// </summary>
        void SetPlanValue(T key, W value);

        /// <summary>
        /// Returns true if planner-scoped value exists.
        /// </summary>
        bool HasPlanValue(T target);

        /// <summary>
        /// Returns all available goals.
        /// Must be thread-safe.
        /// </summary>
        List<IReGoapGoal<T, W>> GetGoalsSet();

        /// <summary>
        /// Returns all available actions.
        /// Must be thread-safe.
        /// </summary>
        List<IReGoapAction<T, W>> GetActionsSet();

        /// <summary>
        /// Creates a new reusable GOAP state instance.
        /// </summary>
        ReGoapState<T, W> InstantiateNewState();

    }
}
