using System;
using System.Collections.Generic;
using System.Linq;
using ReGoap.Core;
using ReGoap.Planner;
using UnityEngine;

// generic goal, should inherit this to do your own goal
namespace ReGoap.Unity
{
    public class ReGoapGoal<T, W> : MonoBehaviour, IReGoapGoal<T, W>
    {
        public string Name = "GenericGoal";
        public float Priority = 1;
        public float ErrorDelay = 0.5f;

        public bool WarnPossibleGoal = true;

        protected ReGoapState<T, W> goal;
        protected Queue<ReGoapActionState<T, W>> plan;
        protected IGoapPlanner<T, W> planner;

        #region UnityFunctions
        protected virtual void Awake()
        {
            goal = ReGoapState<T, W>.Instantiate();
        }

        protected virtual void OnDestroy()
        {
            goal.Recycle();
        }

        protected virtual void Start()
        {
        }
        #endregion

        #region IReGoapGoal
        public virtual string GetName()
        {
            return Name;
        }

        public virtual float GetPriority()
        {
            return Priority;
        }

        public virtual bool IsGoalPossible()
        {
            return WarnPossibleGoal;
        }

        public virtual Queue<ReGoapActionState<T, W>> GetPlan()
        {
            return plan;
        }

        public virtual ReGoapState<T, W> GetGoalState()
        {
            return goal;
        }

        public virtual void SetPlan(Queue<ReGoapActionState<T, W>> path)
        {
            plan = path;
        }

        public void Run(Action<IReGoapGoal<T, W>> callback)
        {
        }

        public virtual void Precalculations(IGoapPlanner<T, W> goapPlanner)
        {
            planner = goapPlanner;
        }

        public virtual float GetErrorDelay()
        {
            return ErrorDelay;
        }
        #endregion

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