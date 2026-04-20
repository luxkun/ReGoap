using System;
using System.Collections.Generic;
using System.Linq;
using ReGoap.Core;
using ReGoap.Utilities;

namespace ReGoap.Godot
{
    /// <summary>
    /// Base Godot GOAP agent implementation.
    /// Manages goal selection, planning requests and action execution transitions.
    /// </summary>
    public partial class ReGoapAgent<T, W> : global::Godot.Node, IReGoapAgent<T, W>, IReGoapAgentHelper
    {
        public new string Name;
        public float CalculationDelay = 0.5f;
        public bool BlackListGoalOnFailure;
        public bool CalculateNewGoalOnStart = true;

        protected float lastCalculationTime;

        protected List<IReGoapGoal<T, W>> goals;
        protected List<IReGoapAction<T, W>> actions;
        protected IReGoapMemory<T, W> memory;
        protected IReGoapGoal<T, W> currentGoal;
        protected ReGoapActionState<T, W> currentActionState;

        protected Dictionary<IReGoapGoal<T, W>, float> goalBlacklist;
        protected List<IReGoapGoal<T, W>> possibleGoals;
        protected bool possibleGoalsDirty;
        protected List<ReGoapActionState<T, W>> startingPlan;
        protected Dictionary<T, W> planValues;
        protected bool interruptOnNextTransition;

        protected bool startedPlanning;
        protected ReGoapPlanWork<T, W> currentReGoapPlanWorker;
        /// <summary>
        /// True while a planner worker is running and has not produced a goal yet.
        /// </summary>
        public bool IsPlanning
        {
            get { return startedPlanning && currentReGoapPlanWorker.NewGoal == null; }
        }

        /// <summary>
        /// Initializes internal collections and optionally starts first planning cycle.
        /// </summary>
        public override void _Ready()
        {
            lastCalculationTime = -100f;
            goalBlacklist = new Dictionary<IReGoapGoal<T, W>, float>();

            RefreshGoalsSet();
            RefreshActionsSet();
            RefreshMemory();

            if (CalculateNewGoalOnStart)
            {
                CalculateNewGoal(true);
            }
        }

        /// <summary>
        /// Stops running action when node exits tree.
        /// </summary>
        public override void _ExitTree()
        {
            if (currentActionState != null)
            {
                currentActionState.Action.Exit(null);
                currentActionState = null;
                currentGoal = null;
            }
        }

        /// <summary>
        /// Time provider used by planning cooldown/blacklist logic.
        /// </summary>
        protected virtual float GetTime()
        {
            return (float)(DateTime.UtcNow.Ticks / (double)TimeSpan.TicksPerSecond);
        }

        /// <summary>
        /// Rebuilds possible-goals list applying blacklist expiration.
        /// </summary>
        protected virtual void UpdatePossibleGoals()
        {
            possibleGoalsDirty = false;
            var time = GetTime();
            if (goalBlacklist.Count > 0)
            {
                possibleGoals = new List<IReGoapGoal<T, W>>(goals.Count);
                foreach (var goal in goals)
                {
                    if (!goalBlacklist.ContainsKey(goal))
                    {
                        possibleGoals.Add(goal);
                    }
                    else if (goalBlacklist[goal] < time)
                    {
                        goalBlacklist.Remove(goal);
                        possibleGoals.Add(goal);
                    }
                }
            }
            else
            {
                possibleGoals = goals;
            }
        }

        /// <summary>
        /// Helper that fails immediately when interruptable, otherwise requests interruption.
        /// </summary>
        protected virtual void TryWarnActionFailure(IReGoapAction<T, W> action)
        {
            if (action.IsInterruptable())
                WarnActionFailure(action);
            else
                action.AskForInterruption();
        }

        /// <summary>
        /// Requests a new planning pass if cooldown allows.
        /// </summary>
        protected virtual bool CalculateNewGoal(bool forceStart = false)
        {
            if (IsPlanning)
                return false;
            var time = GetTime();
            if (!forceStart && (time - lastCalculationTime <= CalculationDelay))
                return false;
            lastCalculationTime = time;

            interruptOnNextTransition = false;
            UpdatePossibleGoals();
            startedPlanning = true;
            currentReGoapPlanWorker = ReGoapPlannerManager<T, W>.Instance.Plan(this, BlackListGoalOnFailure ? currentGoal : null,
                currentGoal != null ? currentGoal.GetPlan() : null, OnDonePlanning);

            return true;
        }

        /// <summary>
        /// Planner completion callback.
        /// Applies new plan and starts first action.
        /// </summary>
        protected virtual void OnDonePlanning(IReGoapGoal<T, W> newGoal)
        {
            startedPlanning = false;
            currentReGoapPlanWorker = default(ReGoapPlanWork<T, W>);
            if (newGoal == null)
            {
                if (currentGoal == null)
                {
                    ReGoapLogger.LogWarning("GoapAgent " + this + " could not find a plan.");
                }
                return;
            }

            if (currentActionState != null)
                currentActionState.Action.Exit(null);
            currentActionState = null;
            currentGoal = newGoal;
            if (startingPlan != null)
            {
                for (int i = 0; i < startingPlan.Count; i++)
                {
                    startingPlan[i].Action.PlanExit(i > 0 ? startingPlan[i - 1].Action : null, i + 1 < startingPlan.Count ? startingPlan[i + 1].Action : null, startingPlan[i].Settings, currentGoal.GetGoalState());
                }
            }
            startingPlan = currentGoal.GetPlan().ToList();
            ClearPlanValues();
            for (int i = 0; i < startingPlan.Count; i++)
            {
                startingPlan[i].Action.PlanEnter(i > 0 ? startingPlan[i - 1].Action : null, i + 1 < startingPlan.Count ? startingPlan[i + 1].Action : null, startingPlan[i].Settings, currentGoal.GetGoalState());
            }
            currentGoal.Run(WarnGoalEnd);
            PushAction();
        }

        /// <summary>
        /// Utility formatter for plan logs/debug output.
        /// </summary>
        public static string PlanToString(IEnumerable<IReGoapAction<T, W>> plan)
        {
            var result = "GoapPlan(";
            var reGoapActions = plan as IReGoapAction<T, W>[] ?? plan.ToArray();
            for (var index = 0; index < reGoapActions.Length; index++)
            {
                var action = reGoapActions[index];
                result += string.Format("'{0}'{1}", action, index + 1 < reGoapActions.Length ? ", " : "");
            }
            result += ")";
            return result;
        }

        /// <summary>
        /// Called by actions when they complete successfully.
        /// </summary>
        public virtual void WarnActionEnd(IReGoapAction<T, W> thisAction)
        {
            if (currentActionState != null && thisAction != currentActionState.Action)
                return;
            PushAction();
        }

        /// <summary>
        /// Transitions to next action in plan or starts replanning when plan is exhausted.
        /// </summary>
        protected virtual void PushAction()
        {
            if (interruptOnNextTransition)
            {
                CalculateNewGoal();
                return;
            }
            var plan = currentGoal.GetPlan();
            if (plan.Count == 0)
            {
                if (currentActionState != null)
                {
                    currentActionState.Action.Exit(currentActionState.Action);
                    currentActionState = null;
                }
                CalculateNewGoal();
            }
            else
            {
                var previous = currentActionState;
                currentActionState = plan.Dequeue();
                IReGoapAction<T, W> next = null;
                if (plan.Count > 0)
                    next = plan.Peek().Action;
                if (previous != null)
                    previous.Action.Exit(currentActionState.Action);
                currentActionState.Action.Run(previous != null ? previous.Action : null, next, currentActionState.Settings, currentGoal.GetGoalState(), WarnActionEnd, WarnActionFailure);
            }
        }

        /// <summary>
        /// Called by actions when they fail.
        /// </summary>
        public virtual void WarnActionFailure(IReGoapAction<T, W> thisAction)
        {
            if (currentActionState != null && thisAction != currentActionState.Action)
            {
                ReGoapLogger.LogWarning(string.Format("[GoapAgent] Action {0} warned for failure but is not current action.", thisAction));
                return;
            }
            if (BlackListGoalOnFailure)
                goalBlacklist[currentGoal] = GetTime() + currentGoal.GetErrorDelay();
            CalculateNewGoal(true);
        }

        /// <summary>
        /// Called when current goal finishes.
        /// </summary>
        public virtual void WarnGoalEnd(IReGoapGoal<T, W> goal)
        {
            if (goal != currentGoal)
            {
                ReGoapLogger.LogWarning(string.Format("[GoapAgent] Goal {0} warned for end but is not current goal.", goal));
                return;
            }
            CalculateNewGoal();
        }

        /// <summary>
        /// Called when a potentially better goal becomes available.
        /// </summary>
        public virtual void WarnPossibleGoal(IReGoapGoal<T, W> goal)
        {
            if ((currentGoal != null) && (goal.GetPriority() <= currentGoal.GetPriority()))
                return;
            if (currentActionState != null && !currentActionState.Action.IsInterruptable())
            {
                interruptOnNextTransition = true;
                currentActionState.Action.AskForInterruption();
            }
            else
                CalculateNewGoal();
        }

        /// <summary>
        /// Returns whether this agent is active in scene tree.
        /// </summary>
        public virtual bool IsActive()
        {
            return IsInsideTree();
        }

        /// <summary>
        /// Returns starting plan snapshot of current goal.
        /// </summary>
        public virtual List<ReGoapActionState<T, W>> GetStartingPlan()
        {
            return startingPlan;
        }

        /// <summary>
        /// Clears planner-scoped plan values dictionary.
        /// </summary>
        protected virtual void ClearPlanValues()
        {
            if (planValues == null)
                planValues = new Dictionary<T, W>();
            else
                planValues.Clear();
        }

        /// <summary>
        /// Gets planner-scoped value.
        /// </summary>
        public virtual W GetPlanValue(T key)
        {
            return planValues[key];
        }

        /// <summary>
        /// Returns true when planner-scoped key exists.
        /// </summary>
        public virtual bool HasPlanValue(T key)
        {
            return planValues.ContainsKey(key);
        }

        /// <summary>
        /// Sets planner-scoped value.
        /// </summary>
        public virtual void SetPlanValue(T key, W value)
        {
            planValues[key] = value;
        }

        /// <summary>
        /// Finds all nodes in agent subtree implementing target interface.
        /// </summary>
        protected virtual IEnumerable<I> GetInterfacesInSubtree<I>() where I : class
        {
            var result = new List<I>();
            var queue = new Queue<global::Godot.Node>();
            queue.Enqueue(this);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                var asInterface = node as I;
                if (asInterface != null)
                    result.Add(asInterface);

                foreach (var childObj in node.GetChildren())
                {
                    var child = childObj as global::Godot.Node;
                    if (child != null)
                        queue.Enqueue(child);
                }
            }
            return result;
        }

        /// <summary>
        /// Refreshes memory reference from subtree components.
        /// </summary>
        public virtual void RefreshMemory()
        {
            memory = GetInterfacesInSubtree<IReGoapMemory<T, W>>().FirstOrDefault();
        }

        /// <summary>
        /// Refreshes goals list from subtree components.
        /// </summary>
        public virtual void RefreshGoalsSet()
        {
            goals = new List<IReGoapGoal<T, W>>(GetInterfacesInSubtree<IReGoapGoal<T, W>>());
            possibleGoalsDirty = true;
        }

        /// <summary>
        /// Refreshes actions list from subtree components.
        /// </summary>
        public virtual void RefreshActionsSet()
        {
            actions = new List<IReGoapAction<T, W>>(GetInterfacesInSubtree<IReGoapAction<T, W>>());
        }

        /// <summary>
        /// Returns goals list, recalculating cached list when dirty.
        /// </summary>
        public virtual List<IReGoapGoal<T, W>> GetGoalsSet()
        {
            if (possibleGoalsDirty)
                UpdatePossibleGoals();
            return possibleGoals;
        }

        /// <summary>
        /// Returns actions list.
        /// </summary>
        public virtual List<IReGoapAction<T, W>> GetActionsSet()
        {
            return actions;
        }

        /// <summary>
        /// Returns memory provider.
        /// </summary>
        public virtual IReGoapMemory<T, W> GetMemory()
        {
            return memory;
        }

        /// <summary>
        /// Returns currently selected goal.
        /// </summary>
        public virtual IReGoapGoal<T, W> GetCurrentGoal()
        {
            return currentGoal;
        }

        /// <summary>
        /// Returns currently running action, if any.
        /// </summary>
        public virtual IReGoapAction<T, W> GetCurrentAction()
        {
            return currentActionState != null ? currentActionState.Action : null;
        }

        /// <summary>
        /// Creates new GOAP state instance.
        /// </summary>
        public virtual ReGoapState<T, W> InstantiateNewState()
        {
            return ReGoapState<T, W>.Instantiate();
        }

        public override string ToString()
        {
            return string.Format("GoapAgent('{0}')", Name);
        }

        /// <summary>
        /// Returns generic type arguments used by this agent implementation.
        /// </summary>
        public virtual Type[] GetGenericArguments()
        {
            return GetType().BaseType.GetGenericArguments();
        }
    }
}
