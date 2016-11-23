using System;
using System.Collections.Generic;

public class ReGoapPlanner : IGoapPlanner
{
    private IReGoapAgent goapAgent;
    private IReGoapGoal currentGoal;
    public bool calculated;
    private readonly AStar<ReGoapState> astar;
    private readonly ReGoapPlannerSettings settings;

    public ReGoapPlanner(ReGoapPlannerSettings settings = null)
    {
        this.settings = settings ?? new ReGoapPlannerSettings();
        astar = new AStar<ReGoapState>(this.settings.maxNodesToExpand);
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
            var leaf = (GoapNode)astar.Run(
                new GoapNode(this, goalState, null, null), goalState, settings.maxIterations, settings.planningEarlyExit);
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

    public ReGoapPlannerSettings GetSettings()
    {
        return settings;
    }
}

public interface IGoapPlanner
{
    IReGoapGoal Plan(IReGoapAgent goapAgent, IReGoapGoal blacklistGoal, Queue<IReGoapAction> currentPlan, Action<IReGoapGoal> callback);
    IReGoapGoal GetCurrentGoal();
    IReGoapAgent GetCurrentAgent();
    bool IsPlanning();
    ReGoapPlannerSettings GetSettings();
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
                foreach (var pair in b.values)
                    a.values[pair.Key] = pair.Value;
                return a;
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
                {
                    object thisValue;
                    values.TryGetValue(pair.Key, out thisValue);
                    var otherValue = pair.Value;
                    if (thisValue == otherValue || (thisValue != null && thisValue.Equals(pair.Value)))
                        return true;
                }
                return false;
            }
    }

    public int MissingDifference(ReGoapState other, int stopAt = int.MaxValue)
    {
        ReGoapState nullGoap = null;
        return MissingDifference(other, ref nullGoap, stopAt);
    }

    // write differences in "difference"
    public int MissingDifference(ReGoapState other, ref ReGoapState difference, int stopAt = int.MaxValue, Func<KeyValuePair<string, object>, object, bool> predicate = null)
    {
        lock (values)
        {
            var count = 0;
            foreach (var pair in values)
            {
                var add = false;
                var valueBool = pair.Value as bool?;
                object otherValue;
                other.values.TryGetValue(pair.Key, out otherValue);
                if (valueBool.HasValue)
                {
                    // we don't need to check otherValue type since every key is supposed to always have same value type
                    var otherValueBool = otherValue == null ? false : (bool)otherValue;
                    if (valueBool.Value != otherValueBool)
                        add = true;
                }
                else // generic version
                {
                    if (pair.Value != otherValue)
                        add = true;
                }
                if (add && (predicate == null || predicate(pair, otherValue)))
                {
                    count++;
                    if (difference != null)
                        difference.values[pair.Key] = pair.Value;
                    if (count >= stopAt)
                        break;
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
            return (T)values[key];
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

    private readonly int heuristicMultiplier = 1;
    private readonly bool backwardSearch;

    public GoapNode(IGoapPlanner planner, ReGoapState goal, GoapNode parent, IReGoapAction action)
    {
        this.planner = planner;
        this.parent = parent;
        this.action = action;
        this.goal = (ReGoapState)goal.Clone();
        this.backwardSearch = planner.GetSettings().backwardSearch;

        if (this.parent != null)
        {
            state = this.parent.GetState();
            // g(node)
            g = parent.GetPathCost();
        }
        else
        {
            state = planner.GetCurrentAgent().GetMemory().GetWorldState();
        }
        state = (ReGoapState)state.Clone();
        if (action != null)
        {
            var effects = (ReGoapState)action.GetEffects(goal).Clone();
            state += effects;
            g += action.GetCost(goal);
        }
        // missing states from goal
        // h(node)
        if (backwardSearch)
        {
            // empirical, giving more importance to heuristic should do better in backward search
            heuristicMultiplier *= 10;
        }
        h = this.goal.MissingDifference(state, ref missingState);
        // we calculate this after getting the heuristic value so actions that gives us goal state will go first
        if (backwardSearch && action != null)
        {
            var diff = new ReGoapState();
            // we need only true preconditions
            action.GetPreconditions(this.goal)
                .MissingDifference(state, ref diff, predicate: (pair, otherValue) => pair.Value.Equals(true));
            this.goal += diff;
        }
        // f(node) = g(node) + h(node)
        cost = g + h * heuristicMultiplier;
        if (backwardSearch) // after calculating the heuristic for astar we change it to the real value
        {
            missingState = new ReGoapState();
            h = this.goal.MissingDifference(state, ref missingState);
        }
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
        {
            if (!backwardSearch ||
                (possibleAction != action && possibleAction.CheckProceduralCondition(planner.GetCurrentAgent(), goal) &&
                 possibleAction.GetEffects(goal).HasAny(missingState)))
            {
                var newGoal = goal;
                // doing this in constructor
                //if (backwardSearch)
                //{
                //    var diff = new ReGoapState();
                //    possibleAction.GetPreconditions(goal).MissingDifference(state, ref diff);
                //    newGoal += diff;
                //}
                result.Add(
                    new GoapNode(
                        planner,
                        newGoal,
                        this,
                        possibleAction));
            }
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
            node = (GoapNode)node.GetParent();
        }
        // we need to order path in backward search
        if (backwardSearch)
        {
            var orderedResults = new List<IReGoapAction>(result.Count);
            var memory = (ReGoapState) planner.GetCurrentAgent().GetMemory().GetWorldState().Clone();
            while (orderedResults.Count < result.Count)
            {
                var index = -1;
                for (int i = 0; i < result.Count; i++)
                {
                    var action = result[i];
                    if (action.GetPreconditions(goal).MissingDifference(memory) == 0)
                    {
                        foreach (var effectsPair in action.GetEffects(goal).GetValues())
                        {
                            memory.Set(effectsPair.Key, effectsPair.Value);
                        }
                        orderedResults.Add(action);
                        index = i;
                    }
                }
                if (index == -1)
                    throw new Exception("[ReGoapNode] Error with plan, could not order it.");
                result.RemoveAt(index);
            }
            result = orderedResults;
            //planner.GetCurrentGoal().GetGoalState().MissingDifference(memory, 1) == 0;
        }
        else
        {
            result.Reverse();
        }
        return new Queue<IReGoapAction>(result);
    }

    private IReGoapAction GetAction()
    {
        return action;
    }

    public List<INode<ReGoapState>> CalculatePath()
    {
        var result = new List<INode<ReGoapState>>();
        var node = (INode<ReGoapState>)this;
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

    public int Priority { get; set; }
    public long InsertionIndex { get; set; }
    public int QueueIndex { get; set; }
}

[Serializable]
public class ReGoapPlannerSettings
{
    // experimental
    public bool backwardSearch = false;
    public bool planningEarlyExit = true;
    public int maxIterations = 100;
    public int maxNodesToExpand = 1000;
}