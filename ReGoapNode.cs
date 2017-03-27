using System;
using System.Collections.Generic;
using System.Linq;

public class ReGoapNode : INode<ReGoapState>
{
    private float cost;
    private IGoapPlanner planner;
    private ReGoapNode parent;
    private IReGoapAction action;
    private IReGoapActionSettings actionSettings;
    private ReGoapState state;
    private ReGoapState goal;
    private float g;
    private float h;

    private float heuristicMultiplier = 1;

    private readonly List<INode<ReGoapState>> expandList;

    private ReGoapNode()
    {
        expandList = new List<INode<ReGoapState>>();
    }

    private void Init(IGoapPlanner planner, ReGoapState newGoal, ReGoapNode parent, IReGoapAction action)
    {
        expandList.Clear();

        this.planner = planner;
        this.parent = parent;
        this.action = action;
        if (action != null)
            actionSettings = action.GetSettings(planner.GetCurrentAgent(), newGoal);

        if (this.parent != null)
        {
            state = this.parent.GetState();
            // g(node)
            g = parent.GetPathCost();
        }
        else
        {
            state = (ReGoapState) planner.GetCurrentAgent().GetMemory().GetWorldState().Clone();
        }

        var nextAction = parent == null ? null : parent.action;
        if (action != null)
        {
            // backward search does NOT support negative preconditions
            // since in backward search we relax the problem all preconditions are valid but are added to the current goal
            var preconditions = action.GetPreconditions(newGoal, nextAction);
            goal = newGoal + preconditions;

            var effects = action.GetEffects(newGoal, nextAction);
            state += effects;
            g += action.GetCost(newGoal, nextAction);

            // removing current action effects from goal, no need to do with to the whole state
            //  since the state is the sum of all the previous actions's effects.
            var missingState = ReGoapState.Instantiate();
            goal.MissingDifference(effects, ref missingState);
            goal.Recycle();
            goal = missingState;

            // this is needed every step to make sure that any precondition is not already satisfied
            //  by the world state
            var worldMissingState = ReGoapState.Instantiate();
            goal.MissingDifference(planner.GetCurrentAgent().GetMemory().GetWorldState(), ref worldMissingState);
            goal.Recycle();
            goal = worldMissingState;
        }
        else
        {
            var diff = ReGoapState.Instantiate();
            newGoal.MissingDifference(state, ref diff);
            goal = diff;
        }
        h = goal.Count;
        // f(node) = g(node) + h(node)
        cost = g + h * heuristicMultiplier;
    }

    #region NodeFactory
    private static Stack<ReGoapNode> cachedNodes;

    public static void Warmup(int count)
    {
        cachedNodes = new Stack<ReGoapNode>(count);
        for (int i = 0; i < count; i++)
        {
            cachedNodes.Push(new ReGoapNode());
        }
    }

    public void Recycle()
    {
        state.Recycle();
        state = null;
        goal.Recycle();
        goal = null;
        cachedNodes.Push(this);
    }

    public static ReGoapNode Instantiate(IGoapPlanner planner, ReGoapState newGoal, ReGoapNode parent, IReGoapAction action)
    {
        if (cachedNodes == null)
        {
            cachedNodes = new Stack<ReGoapNode>();
        }
        ReGoapNode node = cachedNodes.Count > 0 ? cachedNodes.Pop() : new ReGoapNode();
        node.Init(planner, newGoal, parent, action);
        return node;
    }
    #endregion

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
        expandList.Clear();
        var possibleActions = GetPossibleActionsEnumerator();
        while (possibleActions.MoveNext())
        {
            var newGoal = goal;
            expandList.Add(
                ReGoapNode.Instantiate(
                    planner,
                    newGoal,
                    this,
                    possibleActions.Current));
        }
        return expandList;
    }

    private IReGoapAction GetAction()
    {
        return action;
    }

    public Queue<ReGoapActionState> CalculatePath()
    {
        var result = new Queue<ReGoapActionState>();
        CalculatePath(ref result);
        return result;
    }

    public void CalculatePath(ref Queue<ReGoapActionState> result)
    {
        var node = this;
        while (node.GetParent() != null)
        {
            result.Enqueue(new ReGoapActionState(node.action, node.actionSettings));
            node = (ReGoapNode)node.GetParent();
        }
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