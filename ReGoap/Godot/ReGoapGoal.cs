using System;
using System.Collections.Generic;
using System.Linq;
using ReGoap.Core;
using ReGoap.Planner;

namespace ReGoap.Godot
{
    /// <summary>
    /// Base Godot goal component implementing GOAP goal behavior.
    /// </summary>
    public partial class ReGoapGoal<T, W> : global::Godot.Node, IReGoapGoal<T, W>
    {
        public new string Name = "GenericGoal";
        public float Priority = 1;
        public float ErrorDelay = 0.5f;

        public bool WarnPossibleGoal = true;

        protected ReGoapState<T, W> goal;
        protected Queue<ReGoapActionState<T, W>> plan;
        protected IGoapPlanner<T, W> planner;

        /// <summary>
        /// Initializes reusable goal state container.
        /// </summary>
        public override void _Ready()
        {
            goal = ReGoapState<T, W>.Instantiate();
        }

        /// <summary>
        /// Recycles goal state on node teardown.
        /// </summary>
        public override void _ExitTree()
        {
            if (goal != null)
            {
                goal.Recycle();
                goal = null;
            }
        }

        /// <summary>
        /// Returns goal display name.
        /// </summary>
        public virtual string GetName()
        {
            return Name;
        }

        /// <summary>
        /// Returns goal priority used for goal sorting.
        /// </summary>
        public virtual float GetPriority()
        {
            return Priority;
        }

        /// <summary>
        /// Returns whether this goal is currently eligible.
        /// </summary>
        public virtual bool IsGoalPossible()
        {
            return WarnPossibleGoal;
        }

        /// <summary>
        /// Returns currently assigned plan queue.
        /// </summary>
        public virtual Queue<ReGoapActionState<T, W>> GetPlan()
        {
            return plan;
        }

        /// <summary>
        /// Returns desired goal state.
        /// </summary>
        public virtual ReGoapState<T, W> GetGoalState()
        {
            return goal;
        }

        /// <summary>
        /// Stores planner-generated action queue for this goal.
        /// </summary>
        public virtual void SetPlan(Queue<ReGoapActionState<T, W>> path)
        {
            plan = path;
        }

        /// <summary>
        /// Goal runtime hook (optional override).
        /// </summary>
        public virtual void Run(Action<IReGoapGoal<T, W>> callback)
        {
        }

        /// <summary>
        /// Planner-time precomputation hook.
        /// </summary>
        public virtual void Precalculations(IGoapPlanner<T, W> goapPlanner)
        {
            planner = goapPlanner;
        }

        /// <summary>
        /// Returns delay before planner retries this goal after failure.
        /// </summary>
        public virtual float GetErrorDelay()
        {
            return ErrorDelay;
        }

        /// <summary>
        /// Utility method that formats plan actions for logs/debug UI.
        /// </summary>
        public static string PlanToString(IEnumerable<IReGoapAction<T, W>> plan)
        {
            var result = "GoapPlan(";
            var reGoapActions = plan as IReGoapAction<T, W>[] ?? plan.ToArray();
            var end = reGoapActions.Length;
            for (var index = 0; index < end; index++)
            {
                var action = reGoapActions[index];
                result += string.Format("'{0}'{1}", action, index + 1 < end ? ", " : "");
            }
            result += ")";
            return result;
        }

        public override string ToString()
        {
            return string.Format("GoapGoal('{0}')", Name);
        }
    }
}
