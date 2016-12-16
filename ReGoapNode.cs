using System;
using System.Collections.Generic;

public class ReGoapNode : INode<ReGoapState>
{
    private readonly int cost;
    private readonly IGoapPlanner planner;
    private readonly ReGoapNode parent;
    private readonly IReGoapAction action;
    private readonly ReGoapState state;
    private readonly ReGoapState goal;
    private readonly int g;
    private readonly int h;

    private readonly int heuristicMultiplier = 1;
    private readonly bool backwardSearch;

    public ReGoapNode(IGoapPlanner planner, ReGoapState newGoal, ReGoapNode parent, IReGoapAction action)
    {
        this.planner = planner;
        this.parent = parent;
        this.action = action;
        goal = (ReGoapState)newGoal.Clone();
        backwardSearch = planner.GetSettings().backwardSearch;

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
        //state = (ReGoapState)state.Clone(); // no need anymore since ReGoapState add operator now returns a new state
        if (action != null)
        {
            var effects = (ReGoapState)action.GetEffects(goal, parent == null ? null : parent.action).Clone();
            state += effects;
            g += action.GetCost(goal);
        }
        // missing states from goal
        // h(node)
        // we calculate this after getting the heuristic value so actions that gives us goal state will go first
        if (backwardSearch && action != null)
        {
            var diff = new ReGoapState();
            // backward search does NOT support negative preconditions
            action.GetPreconditions(goal)
                .MissingDifference(state, ref diff, predicate: (pair, otherValue) => !pair.Value.Equals(false));
            goal += diff;
        }
        // f(node) = g(node) + h(node)
        cost = g + h * heuristicMultiplier;
        if (backwardSearch) // after calculating the heuristic for astar we change it to the real value
        {
            var missingState = new ReGoapState();
            h = goal.MissingDifference(state, ref missingState);
            goal = missingState;
        }
        else
        {
            h = goal.MissingDifference(state);
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

    public IEnumerator<IReGoapAction> GetPossibleActionsEnumerator()
    {
        var agent = planner.GetCurrentAgent();
        var actions = agent.GetActionsSet();
        for (var index = 0; index < actions.Count; index++)
        {
            var possibleAction = actions[index];
            var precond = possibleAction.GetPreconditions(goal, action);
            var effects = possibleAction.GetEffects(goal, action);
            if (possibleAction == action)
                continue;
            if (backwardSearch)
            {
                if (effects.HasAny(goal) && // any effect is the current goal
                    !goal.HasAnyConflict(effects) && // no effect is conflicting with the goal
                    !goal.HasAnyConflict(precond) && // no precondition is conflicting with the goal
                    possibleAction.CheckProceduralCondition(agent, goal, parent != null ? parent.action : null)) 
                    yield return possibleAction;
            }
            else
            {
                if (precond.MissingDifference(state, 1) == 0 && // check precondition is validated
                    !goal.HasAnyConflict(effects) &&
                    possibleAction.CheckProceduralCondition(agent, goal)) // no effect is conflicting with the goal
                    yield return possibleAction;
            }
        }
    }

    public List<INode<ReGoapState>> Expand()
    {
        var result = new List<INode<ReGoapState>>();
        var possibleActions = GetPossibleActionsEnumerator();
        while (possibleActions.MoveNext())
        {
            var newGoal = goal;
            result.Add(
                new ReGoapNode(
                    planner,
                    newGoal,
                    this,
                    possibleActions.Current));
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
            node = (ReGoapNode)node.GetParent();
        }
        // we need to order path in backward search
        if (backwardSearch)
        {
            var orderedResults = new List<IReGoapAction>(result.Count);
            var memory = (ReGoapState)planner.GetCurrentAgent().GetMemory().GetWorldState().Clone();
            while (orderedResults.Count < result.Count)
            {
                var index = -1;
                for (int i = 0; i < result.Count; i++)
                {
                    var action = result[i];
                    IReGoapAction nextAction = i + 1 < result.Count ? result[i + 1] : null;
                    if (action.GetPreconditions(goal, nextAction).MissingDifference(memory) == 0)
                    {
                        foreach (var effectsPair in action.GetEffects(goal, nextAction).GetValues())
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