using System;
using System.Collections.Generic;

namespace ReGoap.Core
{
    /// <summary>
    /// Planner-time context passed to actions when computing dynamic settings,
    /// preconditions, effects and procedural checks.
    /// </summary>
    public struct GoapActionStackData<T, W>
    {
        public ReGoapState<T, W> currentState;
        public ReGoapState<T, W> goalState;
        public IReGoapAgent<T, W> agent;
        public IReGoapAction<T, W> next;
        public ReGoapState<T, W> settings;
    }

    /// <summary>
    /// Contract implemented by all GOAP actions.
    /// </summary>
    public interface IReGoapAction<T, W>
    {
        /// <summary>
        /// Returns action settings candidates used by the planner.
        /// Dynamic actions can return multiple setting variants.
        /// </summary>
        List<ReGoapState<T, W>> GetSettings(GoapActionStackData<T, W> stackData);

        /// <summary>
        /// Starts action execution.
        /// </summary>
        void Run(IReGoapAction<T, W> previousAction, IReGoapAction<T, W> nextAction, ReGoapState<T, W> settings, ReGoapState<T, W> goalState, Action<IReGoapAction<T, W>> done, Action<IReGoapAction<T, W>> fail);

        /// <summary>
        /// Called when this action becomes part of an accepted plan.
        /// </summary>
        void PlanEnter(IReGoapAction<T, W> previousAction, IReGoapAction<T, W> nextAction, ReGoapState<T, W> settings, ReGoapState<T, W> goalState);

        /// <summary>
        /// Called when the plan containing this action is replaced, fails, or ends.
        /// </summary>
        void PlanExit(IReGoapAction<T, W> previousAction, IReGoapAction<T, W> nextAction, ReGoapState<T, W> settings, ReGoapState<T, W> goalState);

        /// <summary>
        /// Stops this action at runtime.
        /// </summary>
        void Exit(IReGoapAction<T, W> nextAction);

        /// <summary>
        /// Returns display name used in logs/debugger.
        /// </summary>
        string GetName();

        /// <summary>
        /// Returns true while action is currently running.
        /// </summary>
        bool IsActive();

        /// <summary>
        /// Returns whether the action can be interrupted immediately.
        /// </summary>
        bool IsInterruptable();

        /// <summary>
        /// Requests interruption when possible.
        /// </summary>
        void AskForInterruption();

        /// <summary>
        /// Returns action preconditions used by planner expansion.
        /// Must be thread-safe.
        /// </summary>
        ReGoapState<T, W> GetPreconditions(GoapActionStackData<T, W> stackData);

        /// <summary>
        /// Returns action effects used by planner expansion.
        /// Must be thread-safe.
        /// </summary>
        ReGoapState<T, W> GetEffects(GoapActionStackData<T, W> stackData);

        /// <summary>
        /// Additional runtime/planner feasibility check.
        /// </summary>
        bool CheckProceduralCondition(GoapActionStackData<T, W> stackData);

        /// <summary>
        /// Returns action traversal cost for the current stack context.
        /// </summary>
        float GetCost(GoapActionStackData<T, W> stackData);

        /// <summary>
        /// Performs precomputation before planner checks.
        /// Should not mutate runtime execution state.
        /// </summary>
        void Precalculations(GoapActionStackData<T, W> stackData);

        /// <summary>
        /// Debug string with stack-specific details.
        /// </summary>
        string ToString(GoapActionStackData<T, W> stackData);
    }
}
