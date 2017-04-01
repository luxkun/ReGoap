using System;
using System.Collections.Generic;

public class ReGoapPlanner<T, W> : IGoapPlanner<T, W>
{
    private IReGoapAgent<T, W> goapAgent;
    private IReGoapGoal<T, W> currentGoal;
    public bool Calculated;
    private readonly AStar<ReGoapState<T, W>> astar;
    private readonly ReGoapPlannerSettings settings;

    public ReGoapPlanner(ReGoapPlannerSettings settings = null)
    {
        this.settings = settings ?? new ReGoapPlannerSettings();
        astar = new AStar<ReGoapState<T, W>>(this.settings.MaxNodesToExpand);
    }

    public IReGoapGoal<T, W> Plan(IReGoapAgent<T, W> agent, IReGoapGoal<T, W> blacklistGoal = null, Queue<ReGoapActionState<T, W>> currentPlan = null, Action<IReGoapGoal<T, W>> callback = null)
    {
        ReGoapLogger.Log("[ReGoalPlanner] Starting planning calculation for agent: " + agent);
        goapAgent = agent;
        Calculated = false;
        currentGoal = null;
        var possibleGoals = new List<IReGoapGoal<T, W>>();
        foreach (var goal in goapAgent.GetGoalsSet())
        {
            if (goal == blacklistGoal)
                continue;
            goal.Precalculations(this);
            if (goal.IsGoalPossible())
                possibleGoals.Add(goal);
        }
        possibleGoals.Sort((x, y) => x.GetPriority().CompareTo(y.GetPriority()));

        while (possibleGoals.Count > 0)
        {
            currentGoal = possibleGoals[possibleGoals.Count - 1];
            possibleGoals.RemoveAt(possibleGoals.Count - 1);
            var goalState = currentGoal.GetGoalState();

            // can't work with dynamic actions, of course
            if (!settings.UsingDynamicActions)
            {
                var wantedGoalCheck = currentGoal.GetGoalState();
                // we check if the goal can be archived through actions first, so we don't brute force it with A* if we can't
                foreach (var action in goapAgent.GetActionsSet())
                {
                    action.Precalculations(goapAgent, goalState);
                    if (!action.CheckProceduralCondition(goapAgent, wantedGoalCheck))
                    {
                        continue;
                    }
                    // check if the effects of all actions can archieve currentGoal
                    var previous = wantedGoalCheck;
                    wantedGoalCheck = ReGoapState<T, W>.Instantiate();
                    previous.MissingDifference(action.GetEffects(wantedGoalCheck), ref wantedGoalCheck);
                }
                // can't validate goal 
                if (wantedGoalCheck.Count > 0)
                {
                    currentGoal = null;
                    continue;
                }
            }

            goalState = (ReGoapState<T, W>) goalState.Clone();
            var leaf = (ReGoapNode<T, W>)astar.Run(
                ReGoapNode<T, W>.Instantiate(this, goalState, null, null), goalState, settings.MaxIterations, settings.PlanningEarlyExit);
            if (leaf == null)
            {
                currentGoal = null;
                continue;
            }
            var path = leaf.CalculatePath();
            if (currentPlan != null && currentPlan == path)
            {
                currentGoal = null;
                break;
            }
            currentGoal.SetPlan(path);
            break;
        }
        Calculated = true;

        if (callback != null)
            callback(currentGoal);
        if (currentGoal != null)
            ReGoapLogger.Log(string.Format("[ReGoapPlanner] Calculated plan for goal '{0}', plan length: {1}", currentGoal, currentGoal.GetPlan().Count));
        else
            ReGoapLogger.LogWarning("[ReGoapPlanner] Error while calculating plan.");
        return currentGoal;
    }

    public IReGoapGoal<T, W> GetCurrentGoal()
    {
        return currentGoal;
    }

    public IReGoapAgent<T, W> GetCurrentAgent()
    {
        return goapAgent;
    }

    public bool IsPlanning()
    {
        return !Calculated;
    }

    public ReGoapPlannerSettings GetSettings()
    {
        return settings;
    }
}