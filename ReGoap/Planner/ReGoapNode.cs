using System;
using System.Collections.Generic;
using System.Linq;

public class ReGoapNode<T, W> : INode<ReGoapState<T, W>>
{
    private float cost;
    private IGoapPlanner<T, W> planner;
    private ReGoapNode<T, W> parent;
    private IReGoapAction<T, W> action;
    private IReGoapActionSettings<T, W> actionSettings;
    private ReGoapState<T, W> state;
    private ReGoapState<T, W> goal;
    private float g;
    private float h;

    private float heuristicMultiplier = 1;

    private readonly List<INode<ReGoapState<T, W>>> expandList;

    private ReGoapNode()
    {
        expandList = new List<INode<ReGoapState<T, W>>>();
    }

    private void Init(IGoapPlanner<T, W> planner, ReGoapState<T, W> newGoal, ReGoapNode<T, W> parent, IReGoapAction<T, W> action)
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
            state = (ReGoapState<T, W>)planner.GetCurrentAgent().GetMemory().GetWorldState().Clone();
        }

        var nextAction = parent == null ? null : parent.action;
        if (action != null)
        {
            // since in backward search we relax the problem all preconditions are valid but are added to the current goal
            var preconditions = action.GetPreconditions(newGoal, nextAction);
            goal = newGoal + preconditions;

            var effects = action.GetEffects(newGoal, nextAction);
            state += effects;
            g += action.GetCost(newGoal, nextAction);

            // removing current action effects from goal, no need to do with to the whole state
            //  since the state is the sum of all the previous actions's effects.
            var missingState = ReGoapState<T, W>.Instantiate();
            goal.MissingDifference(effects, ref missingState);
            goal.Recycle();
            goal = missingState;

            // this is needed every step to make sure that any precondition is not already satisfied
            //  by the world state
            var worldMissingState = ReGoapState<T, W>.Instantiate();
            goal.MissingDifference(planner.GetCurrentAgent().GetMemory().GetWorldState(), ref worldMissingState);
            goal.Recycle();
            goal = worldMissingState;
        }
        else
        {
            var diff = ReGoapState<T, W>.Instantiate();
            newGoal.MissingDifference(state, ref diff);
            goal = diff;
        }
        h = goal.Count;
        // f(node) = g(node) + h(node)
        cost = g + h * heuristicMultiplier;
    }

    #region NodeFactory
    private static Stack<ReGoapNode<T, W>> cachedNodes;

    public static void Warmup(int count)
    {
        cachedNodes = new Stack<ReGoapNode<T, W>>(count);
        for (int i = 0; i < count; i++)
        {
            cachedNodes.Push(new ReGoapNode<T, W>());
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

    public static ReGoapNode<T, W> Instantiate(IGoapPlanner<T, W> planner, ReGoapState<T, W> newGoal, ReGoapNode<T, W> parent, IReGoapAction<T, W> action)
    {
        if (cachedNodes == null)
        {
            cachedNodes = new Stack<ReGoapNode<T, W>>();
        }
        ReGoapNode<T, W> node = cachedNodes.Count > 0 ? cachedNodes.Pop() : new ReGoapNode<T, W>();
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

    public ReGoapState<T, W> GetState()
    {
        return state;
    }

    public IEnumerator<IReGoapAction<T, W>> GetPossibleActionsEnumerator()
    {
        var agent = planner.GetCurrentAgent();
        var actions = agent.GetActionsSet();
        for (var index = actions.Count - 1; index >= 0; index--)
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

    public List<INode<ReGoapState<T, W>>> Expand()
    {
        expandList.Clear();
        var possibleActions = GetPossibleActionsEnumerator();
        while (possibleActions.MoveNext())
        {
            var newGoal = goal;
            expandList.Add(Instantiate(planner, newGoal, this, possibleActions.Current));
        }
        return expandList;
    }

    private IReGoapAction<T, W> GetAction()
    {
        return action;
    }

    public Queue<ReGoapActionState<T, W>> CalculatePath()
    {
        var result = new Queue<ReGoapActionState<T, W>>();
        CalculatePath(ref result);
        return result;
    }

    public void CalculatePath(ref Queue<ReGoapActionState<T, W>> result)
    {
        var node = this;
        while (node.GetParent() != null)
        {
            result.Enqueue(new ReGoapActionState<T, W>(node.action, node.actionSettings));
            node = (ReGoapNode<T, W>)node.GetParent();
        }
    }

    public int CompareTo(INode<ReGoapState<T, W>> other)
    {
        return cost.CompareTo(other.GetCost());
    }

    public float GetCost()
    {
        return cost;
    }

    public INode<ReGoapState<T, W>> GetParent()
    {
        return parent;
    }

    public bool IsGoal(ReGoapState<T, W> goal)
    {
        return h == 0;
    }

    public float Priority { get; set; }
    public long InsertionIndex { get; set; }
    public int QueueIndex { get; set; }
}