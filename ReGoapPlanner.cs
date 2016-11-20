using System;
using System.Collections.Generic;

public class ReGoapPlanner : IGoapPlanner
{
    private IReGoapAgent goapAgent;
    private IReGoapGoal currentGoal;
    public bool calculated;

    void Start()
    {
    }

    public IReGoapGoal Plan(IReGoapAgent agent, IReGoapGoal blacklistGoal = null, Queue<IReGoapAction> currentPlan = null, Action<IReGoapGoal> callback = null)
    {
        ReGoapLogger.Log("[ReGoalPlanner] Starting planning calculation for agent: " + agent);
        goapAgent = agent;
        calculated = false;
        currentGoal = null;
        var possibleGoals = new List<IReGoapGoal>();
        foreach (var goal in goapAgent.GetGoalsSet())
        {
            if (goal == blacklistGoal)
                continue;
            goal.Precalculations(this);
            if (goal.IsGoalPossible()) //goal.GetPriority() > bestPriority && 
                possibleGoals.Add(goal);
        }
        possibleGoals.Sort((x, y) => x.GetPriority().CompareTo(y.GetPriority()));

        while (possibleGoals.Count > 0)
        {
            currentGoal = possibleGoals[possibleGoals.Count - 1];
            possibleGoals.RemoveAt(possibleGoals.Count - 1);
            var goalState = currentGoal.GetGoalState();

            var wantedGoalCheck = currentGoal.GetGoalState();
            // we check if the goal can be archived through actions first, so we don't brute force it with A* if we can't
            foreach (var action in goapAgent.GetActionsSet())
            {
                action.Precalculations(goapAgent, goalState);
                if (!action.CheckProceduralCondition(goapAgent, wantedGoalCheck))
                    continue;
                var previous = wantedGoalCheck;
                wantedGoalCheck = new ReGoapState();
                previous.MissingDifference(action.GetEffects(wantedGoalCheck), ref wantedGoalCheck);
            }
            // can't validate goal 
            if (wantedGoalCheck.Count > 0)
            {
                currentGoal = null;
                continue;
            }
            var leaf = (GoapNode) AStar.Run(
                new GoapNode(this, goalState, null, null), goalState);
            if (leaf == null)
            {
                currentGoal = null;
                continue;
            }
            var path = leaf.CalculateGoalPath();
            if (currentPlan != null && currentPlan == path)
            {
                currentGoal = null;
                break;
            }
            currentGoal.SetPlan(path);
            break;
        }
        calculated = true;

        if (callback != null)
            callback(currentGoal);
        if (currentGoal != null)
            ReGoapLogger.Log(string.Format("[ReGoapPlanner] Calculated plan for goal '{0}', plan: {1}", currentGoal, currentGoal.GetPlan()));
        else
            ReGoapLogger.LogWarning("[ReGoapPlanner] Error while calculating plan.");
        return currentGoal;
    }

    public IReGoapGoal GetCurrentGoal()
    {
        return currentGoal;
    }

    public IReGoapAgent GetCurrentAgent()
    {
        return goapAgent;
    }

    public bool IsPlanning()
    {
        return !calculated;
    }
}

public interface IGoapPlanner
{
    IReGoapGoal Plan(IReGoapAgent goapAgent, IReGoapGoal blacklistGoal, Queue<IReGoapAction> currentPlan, Action<IReGoapGoal> callback);
    IReGoapGoal GetCurrentGoal();
    IReGoapAgent GetCurrentAgent();
    bool IsPlanning();
}

public class ReGoapState :
    ICloneable
{
    // can change to object
    private volatile Dictionary<string, object> values;

    public ReGoapState(ReGoapState old)
    {
        lock (old.values)
            values = new Dictionary<string, object>(old.values);
    }

    public ReGoapState()
    {
        values = new Dictionary<string, object>();
    }

    public static ReGoapState operator +(ReGoapState a, ReGoapState b)
    {
        lock (a.values)
            lock (b.values)
            {
                var state = (ReGoapState) a.Clone();
                foreach (var pair in b.values)
                    state.values[pair.Key] = pair.Value;
                return state;
            }
    }

    public int Count
    {
        get { return values.Count; }
    }
    public bool HasAny(ReGoapState other)
    {
        lock (values) lock (other.values)
        {
            foreach (var pair in other.values)
                if (values.ContainsKey(pair.Key) && (values[pair.Key] == other.values[pair.Key]))
                    return true;
            return false;
        }
    }

    public int MissingDifference(ReGoapState other, int stopAt = int.MaxValue)
    {
        ReGoapState nullGoap = null;
        return MissingDifference(other, ref nullGoap, stopAt);
    }

    // write differences in "difference"
    public int MissingDifference(ReGoapState other, ref ReGoapState difference, int stopAt = int.MaxValue)
    {
        lock (values)
        {
            var count = 0;
            foreach (var pair in values)
            {
                var add = false;
                if (pair.Value is bool)
                {
                    if ((!(bool) pair.Value && other.values.ContainsKey(pair.Key) && (bool) other.values[pair.Key]) ||
                        ((bool) pair.Value && (!other.values.ContainsKey(pair.Key) || !(bool) other.values[pair.Key])))
                        add = true;
                }
                else // generic version
                {

                    if ((pair.Value == null && other.values.ContainsKey(pair.Key) && other.values[pair.Key] != null) ||
                        (pair.Value != null &&
                         (!other.values.ContainsKey(pair.Key) || other.values[pair.Key] != pair.Value)))
                        add = true;
                }
                if (add)
                {
                    count++;
                    if (count >= stopAt)
                        break;
                    if (difference != null)
                        difference.values[pair.Key] = pair.Value;
                }
            }
            return count;
        }
    }
    public object Clone()
    {
        var clone = new ReGoapState(this);
        return clone;
    }

    public override string ToString()
    {
        lock (values)
        {
            var result = "GoapState: ";
            foreach (var pair in values)
                result += string.Format("'{0}': {1}, ", pair.Key, pair.Value);
            return result;
        }
    }

    public T Get<T>(string key)
    {
        lock (values)
        {
            if (!values.ContainsKey(key))
                return default(T);
            return (T) values[key];
        }
    }

    public void Set<T>(string key, T value)
    {
        lock (values)
        {
            values[key] = value;
        }
    }

    public void Remove(string key)
    {
        lock (values)
        {
            values.Remove(key);
        }
    }

    public Dictionary<string, object> GetValues()
    {
        lock (values) 
            return values;
    }

    public bool HasKey(string key)
    {
        lock (values)
            return values.ContainsKey(key);
    }
}

public class GoapNode : INode<ReGoapState>
{
    private readonly int cost;
    private readonly IGoapPlanner planner;
    private readonly GoapNode parent;
    private readonly IReGoapAction action;
    private readonly ReGoapState state;
    private readonly ReGoapState goal;
    private readonly int g;
    private readonly int h;
    private readonly ReGoapState missingState;

    // experimental: not working
    // TODO: backward search
    public bool backwardSearch = false;
    private readonly int heuristicMultiplier = 1;

    public GoapNode(IGoapPlanner p, ReGoapState go, GoapNode par, IReGoapAction a)
    {
        planner = p;
        parent = par;
        action = a;
        goal = go;

        if (parent != null)
        {
            state = parent.GetState();
            // g(node)
            g = par.GetPathCost();
        }
        else
        {
            state = p.GetCurrentAgent().GetMemory().GetWorldState();
        }
        if (action != null)
        {
            state += a.GetEffects(state);
            g += a.GetCost(state);
        }
        // missing states from goal
        // h(node)
        if (backwardSearch)
            missingState = new ReGoapState();
        h = goal.MissingDifference(state, ref missingState);
        // f(node) = g(node) + h(node)
        cost = g + h*heuristicMultiplier;
    }

    public int GetPathCost()
    {
        return g;
    }

    public int GetHeuristicCost()
    {
        return h;
    }

    public ReGoapState GetState()
    {
        return state;
    }

    public List<IReGoapAction> GetPossibleActionsSet()
    {
        var result = new List<IReGoapAction>();
        var agent = planner.GetCurrentAgent();
        var actions = agent.GetActionsSet();
        for (var index = 0; index < actions.Count; index++)
        {
            var act = actions[index];
            var precond = act.GetPreconditions(goal);
            if ((precond.MissingDifference(state, 1) == 0) && act.CheckProceduralCondition(agent, goal))
                result.Add(act);
        }
        return result;
    }

    public List<INode<ReGoapState>> Expand()
    {
        var result = new List<INode<ReGoapState>>();
        List<IReGoapAction> actions;
        if (backwardSearch)
            actions = planner.GetCurrentAgent().GetActionsSet();
        else
            actions = GetPossibleActionsSet();
        foreach (var possibleAction in actions)
            if (!backwardSearch || missingState.HasAny(goal))
            {
                var newGoal = goal;
                // theorically relax the problem for backward search, not working yet
                if (backwardSearch)
                {
                    var diff = new ReGoapState();
                    possibleAction.GetPreconditions(goal).MissingDifference(state, ref diff);
                    newGoal += diff;
                }
                result.Add(
                    new GoapNode(
                        planner,
                        newGoal,
                        this,
                        possibleAction));
            }
        return result;
    }

    public Queue<IReGoapAction> CalculateGoalPath()
    {
        var result = new List<IReGoapAction>();
        var node = this;
        while (node != null)
        {
            if (node.GetAction() != null) //first
                result.Add(node.GetAction());
            node = (GoapNode) node.GetParent();
        }
        result.Reverse();
        return new Queue<IReGoapAction>(result);
    }

    private IReGoapAction GetAction()
    {
        return action;
    }

    public List<INode<ReGoapState>> CalculatePath()
    {
        var result = new List<INode<ReGoapState>>();
        var node = (INode<ReGoapState>) this;
        while (node.GetParent() != null)
        {
            result.Add(node);
            node = node.GetParent();
        }
        result.Reverse();
        return result;
    }

    public int CompareTo(INode<ReGoapState> other)
    {
        return cost - other.GetCost();
    }

    public int GetCost()
    {
        return cost;
    }

    public INode<ReGoapState> GetParent()
    {
        return parent;
    }

    public bool IsGoal(ReGoapState goal)
    {
        return h == 0;
    }

    public double Priority { get; set; }
    public long InsertionIndex { get; set; }
    public int QueueIndex { get; set; }
}