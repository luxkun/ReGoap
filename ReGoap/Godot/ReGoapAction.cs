using System;
using System.Collections.Generic;
using ReGoap.Core;

namespace ReGoap.Godot
{
    /// <summary>
    /// Base Godot action component implementing the ReGoap action contract.
    /// </summary>
    public partial class ReGoapAction<T, W> : global::Godot.Node, IReGoapAction<T, W>
    {
        public new string Name = "GoapAction";

        protected ReGoapState<T, W> preconditions;
        protected ReGoapState<T, W> effects;
        public float Cost = 1;

        protected Action<IReGoapAction<T, W>> doneCallback;
        protected Action<IReGoapAction<T, W>> failCallback;
        protected IReGoapAction<T, W> previousAction;
        protected IReGoapAction<T, W> nextAction;

        protected IReGoapAgent<T, W> agent;
        protected bool interruptWhenPossible;
        protected ReGoapState<T, W> settings;
        protected bool active;

        /// <summary>
        /// Initializes internal reusable state containers.
        /// </summary>
        public override void _Ready()
        {
            active = false;
            effects = ReGoapState<T, W>.Instantiate();
            preconditions = ReGoapState<T, W>.Instantiate();
            settings = ReGoapState<T, W>.Instantiate();
        }

        /// <summary>
        /// Returns whether action is currently executing.
        /// </summary>
        public virtual bool IsActive()
        {
            return active;
        }

        /// <summary>
        /// Called after planning so action can cache agent references.
        /// </summary>
        public virtual void PostPlanCalculations(IReGoapAgent<T, W> goapAgent)
        {
            agent = goapAgent;
        }

        /// <summary>
        /// Returns whether the action can be interrupted immediately.
        /// </summary>
        public virtual bool IsInterruptable()
        {
            return true;
        }

        /// <summary>
        /// Requests interruption when action reaches a safe point.
        /// </summary>
        public virtual void AskForInterruption()
        {
            interruptWhenPossible = true;
        }

        /// <summary>
        /// Planner-time precomputation hook.
        /// </summary>
        public virtual void Precalculations(GoapActionStackData<T, W> stackData)
        {
            agent = stackData.agent;
        }

        /// <summary>
        /// Returns candidate setting list for planner expansion.
        /// </summary>
        public virtual List<ReGoapState<T, W>> GetSettings(GoapActionStackData<T, W> stackData)
        {
            return new List<ReGoapState<T, W>> { settings };
        }

        /// <summary>
        /// Returns action preconditions.
        /// </summary>
        public virtual ReGoapState<T, W> GetPreconditions(GoapActionStackData<T, W> stackData)
        {
            return preconditions;
        }

        /// <summary>
        /// Returns action effects.
        /// </summary>
        public virtual ReGoapState<T, W> GetEffects(GoapActionStackData<T, W> stackData)
        {
            return effects;
        }

        /// <summary>
        /// Returns action traversal cost.
        /// </summary>
        public virtual float GetCost(GoapActionStackData<T, W> stackData)
        {
            return Cost;
        }

        /// <summary>
        /// Returns additional procedural feasibility.
        /// </summary>
        public virtual bool CheckProceduralCondition(GoapActionStackData<T, W> stackData)
        {
            return true;
        }

        /// <summary>
        /// Starts runtime execution for this action.
        /// </summary>
        public virtual void Run(IReGoapAction<T, W> previous, IReGoapAction<T, W> next, ReGoapState<T, W> actionSettings,
            ReGoapState<T, W> goalState, Action<IReGoapAction<T, W>> done, Action<IReGoapAction<T, W>> fail)
        {
            interruptWhenPossible = false;
            active = true;
            doneCallback = done;
            failCallback = fail;
            settings = actionSettings;

            previousAction = previous;
            nextAction = next;
        }

        /// <summary>
        /// Plan lifecycle callback when this action is inserted in current plan.
        /// </summary>
        public virtual void PlanEnter(IReGoapAction<T, W> previousAction, IReGoapAction<T, W> nextAction, ReGoapState<T, W> actionSettings, ReGoapState<T, W> goalState)
        {
        }

        /// <summary>
        /// Plan lifecycle callback when this action leaves current plan.
        /// </summary>
        public virtual void PlanExit(IReGoapAction<T, W> previousAction, IReGoapAction<T, W> nextAction, ReGoapState<T, W> actionSettings, ReGoapState<T, W> goalState)
        {
        }

        /// <summary>
        /// Ends runtime action execution.
        /// </summary>
        public virtual void Exit(IReGoapAction<T, W> next)
        {
            active = false;
        }

        /// <summary>
        /// Returns action display name.
        /// </summary>
        public virtual string GetName()
        {
            return Name;
        }

        public override string ToString()
        {
            return string.Format("GoapAction('{0}')", Name);
        }

        /// <summary>
        /// Returns stack-aware debug string.
        /// </summary>
        public virtual string ToString(GoapActionStackData<T, W> stackData)
        {
            var result = string.Format("GoapAction('{0}')", Name);
            if (stackData.settings != null && stackData.settings.Count > 0)
            {
                result += " - ";
                foreach (var pair in stackData.settings.GetValues())
                {
                    result += string.Format("{0}='{1}' ; ", pair.Key, pair.Value);
                }
            }
            return result;
        }
    }
}
