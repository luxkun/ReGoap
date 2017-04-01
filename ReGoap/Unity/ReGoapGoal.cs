using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// generic goal, should inherit this to do your own goal
public class ReGoapGoal<T, W> : MonoBehaviour, IReGoapGoal<T, W>
{
    public string Name = "GenericGoal";
    public float Priority = 1;
    public float ErrorDelay = 0.5f;

    protected ReGoapState<T, W> goal;
    protected Queue<ReGoapActionState<T, W>> plan;
    protected IGoapPlanner<T, W> planner;

    public float WarnDelay = 2f;
    private float warnCooldown;

    #region UnityFunctions
    protected virtual void Awake()
    {
        goal = ReGoapState<T, W>.Instantiate();
    }

    void OnDestroy()
    {
        goal.Recycle();
    }

    protected virtual void Start()
    {
    }

    protected virtual void Update()
    {
    }

    protected virtual void FixedUpdate()
    {
        if ((planner != null) && !planner.IsPlanning() && Time.time > warnCooldown)
        {
            warnCooldown = Time.time + WarnDelay;
            // check if this goal is not active but CAN be activated
            if (((ReGoapGoal<T, W>) planner.GetCurrentGoal() != this) && IsGoalPossible())
                planner.GetCurrentAgent().WarnPossibleGoal(this);
            // if this goal is active but isn't anymore possible
            if (((ReGoapGoal<T, W>) planner.GetCurrentGoal() == this) && !IsGoalPossible())
                planner.GetCurrentAgent().WarnPossibleGoal(this);
        }
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
        return true;
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