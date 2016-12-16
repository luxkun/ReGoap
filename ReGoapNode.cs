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

    public ReGoapNode(IGoapPlanner planner, ReGoapState newGoal, ReGoapNode parent, IReGoapAction action)
    {
        this.planner = planner;
        this.parent = parent;
        this.action = action;
        goal = (ReGoapState)newGoal.Clone();

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
            var nextAction = parent == null ? null : parent.action;
            var effects = (ReGoapState)action.GetEffects(goal, nextAction).Clone();
            state += effects;
            g += action.GetCost(goal, action);
        }
        // missing states from goal
        // h(node)
        // we calculate this after getting the heuristic value so actions that gives us goal state will go first
        if (action != null)
        {
            var diff = new ReGoapState();
            // backward search does NOT support negative preconditions
            action.GetPreconditions(goal)
                .MissingDifference(state, ref diff, predicate: (pair, otherValue) => !pair.Value.Equals(false));
            goal += diff;
        }
        // f(node) = g(node) + h(node)
        cost = g + h * heuristicMultiplier;
        var missingState = new ReGoapState();
        h = goal.MissingDifference(state, ref missingState);
        goal = missingState;
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
            if (effects.HasAny(goal) && // any effect is the current goal
                !goal.HasAnyConflict(effects) && // no effect is conflicting with the goal
                !goal.HasAnyConflict(precond) && // no precondition is conflicting with the goal
                possibleAction.CheckProceduralCondition(agent, goal, parent != null ? parent.action : null)) 
                yield return possibleAction;
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
        var orderedResults = new List<IReGoapAction>(result.Count);
        var memory = (ReGoapState)planner.GetCurrentAgent().GetMemory().GetWorldState().Clone();
        while (orderedResults.Count < result.Count)
        {
            var index = -1;
            for (int i = 0; i < result.Count; i++)
            {
                var thisAction = result[i];
                IReGoapAction nextAction = i + 1 < result.Count ? result[i + 1] : null;
                if (thisAction.GetPreconditions(goal, nextAction).MissingDifference(memory) == 0)
                {
                    foreach (var effectsPair in thisAction.GetEffects(goal, nextAction).GetValues())
                    {
                        memory.Set(effectsPair.Key, effectsPair.Value);
                    }
                    orderedResults.Add(thisAction);
                    index = i;
                }
            }
            if (index == -1)
                throw new Exception("[ReGoapNode] Error with plan, could not order it.");
            result.RemoveAt(index);
        }
        result = orderedResults;
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