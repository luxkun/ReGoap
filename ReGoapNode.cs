using System;
using System.Collections.Generic;
using System.Linq;

public class ReGoapNode : INode<ReGoapState>
{
    private readonly float cost;
    private readonly IGoapPlanner planner;
    private readonly ReGoapNode parent;
    private readonly IReGoapAction action;
    private readonly IReGoapActionSettings actionSettings;
    private readonly ReGoapState state;
    private readonly ReGoapState goal;
    private readonly float g;
    private readonly float h;

    private readonly float heuristicMultiplier = 1;

    public ReGoapNode(IGoapPlanner planner, ReGoapState newGoal, ReGoapNode parent, IReGoapAction action)
    {
        this.planner = planner;
        this.parent = parent;
        this.action = action;
        goal = (ReGoapState)newGoal.Clone();
        if (action != null)
            actionSettings = action.GetSettings(planner.GetCurrentAgent(), goal);

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

        var nextAction = parent == null ? null : parent.action;
        //state = (ReGoapState)state.Clone(); // no need anymore since ReGoapState add operator now returns a new state
        if (action != null)
        {
            var effects = (ReGoapState)action.GetEffects(goal, nextAction).Clone();
            state += effects;
            g += action.GetCost(goal, nextAction);
        }
        // missing states from goal
        // h(node)
        // we calculate this after getting the heuristic value so actions that gives us goal state will go first
        if (action != null)
        {
            var diff = new ReGoapState();
            // backward search does NOT support negative preconditions
            h = action.GetPreconditions(goal, nextAction)
                .MissingDifference(state, ref diff, predicate: (pair, otherValue) => !pair.Value.Equals(false), test: true);
            goal += diff;
        }
        // f(node) = g(node) + h(node)
        cost = g + h * heuristicMultiplier;
        var missingState = new ReGoapState();
        h += goal.MissingDifference(state, ref missingState);
        goal = missingState;
    }

    public float GetPathCost()
    {
        return g;
    }

    public float GetHeuristicCost()
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
            possibleAction.Precalculations(agent, goal);
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

    private IReGoapAction GetAction()
    {
        return action;
    }

    public Queue<ReGoapActionState> CalculatePath()
    {
        var listResult = new List<ReGoapActionState>();
        var node = this;
        while (node.GetParent() != null)
        {
            listResult.Add(new ReGoapActionState(node.action, node.actionSettings));
            node = (ReGoapNode)node.GetParent();
        }
        var result = new Queue<ReGoapActionState>(listResult.Count);
        foreach (var thisActionState in listResult)
        {
            result.Enqueue(thisActionState);
        }
        return result;
    }

    public int CompareTo(INode<ReGoapState> other)
    {
        return cost.CompareTo(other.GetCost());
    }

    public float GetCost()
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

    public float Priority { get; set; }
    public long InsertionIndex { get; set; }
    public int QueueIndex { get; set; }
}