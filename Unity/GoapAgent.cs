using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GoapAgent : MonoBehaviour, IReGoapAgent
{
    public string Name;
    public float calculationDelay = 0.5f;
    public bool blackListGoalOnFailure;

    protected float lastCalculationTime;

    protected List<IReGoapGoal> goals;
    protected List<IReGoapAction> actions;
    protected IReGoapMemory memory;
    protected IReGoapGoal currentGoal;

    protected IReGoapAction currentAction;

    protected Dictionary<IReGoapGoal, float> goalBlacklist;
    protected List<IReGoapGoal> possibleGoals;
    protected bool possibleGoalsDirty;
    protected List<IReGoapAction> startingPlan;
    protected Dictionary<string, object> planValues;
    protected bool interruptOnNextTransistion;

    protected PlanWork? currentPlanWorker;
    public bool isPlanning
    {
        get { return currentPlanWorker != null && currentPlanWorker.Value.newGoal == null; }
    }

    protected virtual void Awake()
    {
        lastCalculationTime = -100;
        goalBlacklist = new Dictionary<IReGoapGoal, float>();

        RefreshGoalsSet();
        RefreshActionsSet();
        RefreshMemory();
    }

    protected virtual void Start()
    {
        CalculateNewGoal();
    }

    protected virtual void UpdatePossibleGoals()
    {
        possibleGoalsDirty = false;
        if (goalBlacklist.Count > 0)
        {
            possibleGoals = new List<IReGoapGoal>(goals.Count);
            foreach (var goal in goals)
                if (!goalBlacklist.ContainsKey(goal))
                {
                    possibleGoals.Add(goal);
                }
                else if (goalBlacklist[goal] < Time.time)
                {
                    goalBlacklist.Remove(goal);
                    possibleGoals.Add(goal);
                }
        }
        else
        {
            possibleGoals = goals;
        }
    }

    protected virtual void FixedUpdate()
    {
        possibleGoalsDirty = true;

        if (currentAction == null)
        {
            if (!isPlanning)
                CalculateNewGoal();
            return;
        }
        // check if current action preconditions are still valid, else invalid action and restart planning
        var state = memory.GetWorldState();
        if (currentAction.GetPreconditions(state).MissingDifference(state, 1) > 0)
            TryWarnActionFailure(currentAction);
    }

    protected virtual void TryWarnActionFailure(IReGoapAction action)
    {
        if (action.IsInterruptable())
            WarnActionFailure(action);
        else
            action.AskForInterruption();
    }

    protected virtual bool CalculateNewGoal(bool forceStart = false)
    {
        if (isPlanning)
            return false;
        if (!forceStart && (Time.time - lastCalculationTime <= calculationDelay))
            return false;
        lastCalculationTime = Time.time;

        interruptOnNextTransistion = false;
        UpdatePossibleGoals();
        //var watch = System.Diagnostics.Stopwatch.StartNew();
        currentPlanWorker = GoapPlannerManager.instance.Plan(this, blackListGoalOnFailure ? currentGoal : null,
            currentGoal != null ? currentGoal.GetPlan() : null, OnDonePlanning);

        return true;
    }

    protected virtual void OnDonePlanning(IReGoapGoal newGoal)
    {
        currentPlanWorker = null;
        if (newGoal == null)
            if (currentGoal == null)
                throw new UnityException("GoapAgent " + this + " could not find a plan.");
            else
                return;

        if (currentAction != null)
            currentAction.Exit(null);
        currentAction = null;
        currentGoal = newGoal;
        var plan = currentGoal.GetPlan();
        startingPlan = plan.ToList();
        ClearPlanValues();
        foreach (var action in startingPlan)
        {
            action.PostPlanCalculations(this);
        }
        currentGoal.Run(WarnGoalEnd);
        PushAction();
    }

    public static string PlanToString(IEnumerable<IReGoapAction> plan)
    {
        var result = "GoapPlan(";
        var reGoapActions = plan as IReGoapAction[] ?? plan.ToArray();
        for (var index = 0; index < reGoapActions.Length; index++)
        {
            var action = reGoapActions[index];
            result += string.Format("'{0}'{1}", action, index + 1 < reGoapActions.Length ? ", " : "");
        }
        result += ")";
        return result;
    }

    public virtual void WarnActionEnd(IReGoapAction action)
    {
        if (action != currentAction)
            return;
        PushAction();
    }

    protected virtual void PushAction()
    {
        if (interruptOnNextTransistion)
        {
            CalculateNewGoal();
            return;
        }
        var plan = currentGoal.GetPlan();
        if (plan.Count == 0)
        {
            CalculateNewGoal();
        }
        else
        {
            var previous = currentAction;
            currentAction = plan.Dequeue();
            IReGoapAction next = null;
            if (plan.Count > 0)
                next = plan.Peek();
            if (previous != null)
                previous.Exit(currentAction);
            currentAction.Run(previous, next, currentGoal.GetGoalState(), WarnActionEnd, WarnActionFailure);
        }
    }

    public virtual void WarnActionFailure(IReGoapAction action)
    {
        if (action != currentAction)
        {
            ReGoapLogger.LogWarning(string.Format("[GoapAgent] Action {0} warned for failure but is not current action.", action));
            return;
        }
        if (blackListGoalOnFailure)
            goalBlacklist[currentGoal] = Time.time + currentGoal.GetErrorDelay();
        CalculateNewGoal(true);
    }

    public virtual void WarnGoalEnd(IReGoapGoal goal)
    {
        if (goal != currentGoal)
        {
            ReGoapLogger.LogWarning(string.Format("[GoapAgent] Goal {0} warned for end but is not current goal.", goal));
            return;
        }
        CalculateNewGoal();
    }

    public virtual void WarnPossibleGoal(IReGoapGoal goal)
    {
        if ((currentGoal != null) && (goal.GetPriority() <= currentGoal.GetPriority()))
            return;
        if (currentAction != null && !currentAction.IsInterruptable())
        {
            interruptOnNextTransistion = true;
            currentAction.AskForInterruption();
        }
        else
            CalculateNewGoal();
    }

    public virtual bool IsActive()
    {
        return enabled;
    }

    public virtual List<IReGoapAction> GetStartingPlan()
    {
        return startingPlan;
    }

    protected virtual void ClearPlanValues()
    {
        if (planValues == null)
            planValues = new Dictionary<string, object>();
        else
        {
            planValues.Clear();
        }
    }

    public virtual T GetPlanValue<T>(string key)
    {
        return (T)planValues[key];
    }

    public virtual bool HasPlanValue(string key)
    {
        return planValues.ContainsKey(key);
    }

    public virtual void SetPlanValue<T>(string key, T value)
    {
        planValues[key] = value;
    }

    public virtual void RefreshMemory()
    {
        memory = GetComponent<IReGoapMemory>();
    }

    public virtual void RefreshGoalsSet()
    {
        goals = new List<IReGoapGoal>(GetComponents<IReGoapGoal>());
        possibleGoalsDirty = true;
    }

    public virtual void RefreshActionsSet()
    {
        actions = new List<IReGoapAction>(GetComponents<IReGoapAction>());
    }

    public virtual List<IReGoapGoal> GetGoalsSet()
    {
        if (possibleGoalsDirty)
            UpdatePossibleGoals();
        return possibleGoals;
    }

    public virtual List<IReGoapAction> GetActionsSet()
    {
        return actions;
    }

    public virtual IReGoapMemory GetMemory()
    {
        return memory;
    }

    public virtual IReGoapGoal GetCurrentGoal()
    {
        return currentGoal;
    }

    public virtual GameObject GetGameObject()
    {
        return gameObject;
    }

    public override string ToString()
    {
        return string.Format("GoapAgent('{0}')", Name);
    }
}