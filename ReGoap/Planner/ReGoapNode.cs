using System.Collections.Generic;
using ReGoap.Core;

namespace ReGoap.Planner
{
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

            if (parent != null)
            {
                state = parent.GetState().Clone();
                // g(node)
                g = parent.GetPathCost();
            }
            else
            {
                state = planner.GetCurrentAgent().GetMemory().GetWorldState().Clone();
            }

            var nextAction = parent == null ? null : parent.action;
            if (action != null)
            {
                // create a new instance of the goal based on the paren't goal
                goal = ReGoapState<T, W>.Instantiate(newGoal);

                var preconditions = action.GetPreconditions(goal, nextAction);
                var effects = action.GetEffects(goal, nextAction);
                // adding the action's effects to the current node's state
                state.AddFromState(effects);
                // addding the action's cost to the node's total cost
                g += action.GetCost(goal, nextAction);

                // add all preconditions of the current action to the goal
                goal.AddFromState(preconditions);
                // removes from goal all the conditions that are now fullfiled in the node's state
                goal.ReplaceWithMissingDifference(state);
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
            lock (cachedNodes)
            {
                cachedNodes.Push(this);
            }
        }

        public static ReGoapNode<T, W> Instantiate(IGoapPlanner<T, W> planner, ReGoapState<T, W> newGoal, ReGoapNode<T, W> parent, IReGoapAction<T, W> action)
        {
            ReGoapNode<T, W> node;
            if (cachedNodes == null)
            {
                cachedNodes = new Stack<ReGoapNode<T, W>>();
            }
            lock (cachedNodes)
            {
                node = cachedNodes.Count > 0 ? cachedNodes.Pop() : new ReGoapNode<T, W>();
            }
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

        public List<INode<ReGoapState<T, W>>> Expand()
        {
            expandList.Clear();

            var agent = planner.GetCurrentAgent();
            var actions = agent.GetActionsSet();
            for (var index = actions.Count - 1; index >= 0; index--)
            {
                var possibleAction = actions[index];
                possibleAction.Precalculations(agent, goal);
                var precond = possibleAction.GetPreconditions(goal, action);
                var effects = possibleAction.GetEffects(goal, action);

                if (effects.HasAny(goal) && // any effect is the current goal
                    !goal.HasAnyConflict(effects, precond) && // no precondition is conflicting with the goal
                    possibleAction.CheckProceduralCondition(agent, goal, parent != null ? parent.action : null))
                {
                    var newGoal = goal;
                    expandList.Add(Instantiate(planner, newGoal, this, possibleAction));
                }
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
            return h <= 0;
        }

        public float Priority { get; set; }
        public long InsertionIndex { get; set; }
        public int QueueIndex { get; set; }
    }
}