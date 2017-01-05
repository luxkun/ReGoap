using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// generic goal, should inherit this to do your own goal
public class GoapGoal : MonoBehaviour, IReGoapGoal
{
    public string Name = "GenericGoal";
    public float Priority = 1;
    public float ErrorDelay = 0.5f;

    protected ReGoapState goal;
    protected Queue<ReGoapActionState> plan;
    protected IGoapPlanner planner;

    public float WarnDelay = 2f;
    private float warnCooldown;

    #region UnityFunctions
    protected virtual void Awake()
    {
        goal = new ReGoapState();
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
            if (((GoapGoal) planner.GetCurrentGoal() != this) && IsGoalPossible())
                planner.GetCurrentAgent().WarnPossibleGoal(this);
            // if this goal is active but isn't anymore possible
            if (((GoapGoal) planner.GetCurrentGoal() == this) && !IsGoalPossible())
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

    public virtual Queue<ReGoapActionState> GetPlan()
    {
        return plan;
    }

    public virtual ReGoapState GetGoalState()
    {
        return goal;
    }

    public virtual void SetPlan(Queue<ReGoapActionState> path)
    {
        plan = path;
    }

    public void Run(Action<IReGoapGoal> callback)
    {
    }

    public virtual void Precalculations(IGoapPlanner goapPlanner)
    {
        planner = goapPlanner;
    }

    public virtual float GetErrorDelay()
    {
        return ErrorDelay;
    }
    #endregion

    public static string PlanToString(IEnumerable<IReGoapAction> plan)
    {
        var result = "GoapPlan(";
        var reGoapActions = plan as IReGoapAction[] ?? plan.ToArray();
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