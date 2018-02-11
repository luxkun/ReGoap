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
        private ReGoapState<T, W> actionSettings;
        private ReGoapState<T, W> state;
        private ReGoapState<T, W> goal;
        private float g;
        private float h;
        private ReGoapState<T, W> goalMergedWithWorld;

        private float heuristicMultiplier = 1;

        private readonly List<INode<ReGoapState<T, W>>> expandList;

        private ReGoapNode()
        {
            expandList = new List<INode<ReGoapState<T, W>>>();
        }

        private void Init(IGoapPlanner<T, W> planner, ReGoapState<T, W> newGoal, ReGoapNode<T, W> parent, IReGoapAction<T, W> action, ReGoapState<T, W> settings)
        {
            expandList.Clear();

            this.planner = planner;
            this.parent = parent;
            this.action = action;
            if (settings != null)
                this.actionSettings = settings.Clone();

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

                GoapActionStackData<T, W> stackData;
                stackData.currentState = state;
                stackData.goalState = goal;
                stackData.next = action;
                stackData.agent = planner.GetCurrentAgent();
                stackData.settings = actionSettings;

                var preconditions = action.GetPreconditions(stackData);
                var effects = action.GetEffects(stackData);
                // addding the action's cost to the node's total cost
                g += action.GetCost(stackData);

                // adding the action's effects to the current node's state
                state.AddFromState(effects);

                // removes from goal all the conditions that are now fullfiled in the action's effects
                goal.ReplaceWithMissingDifference(effects);
                // add all preconditions of the current action to the goal
                goal.AddFromState(preconditions);
            }
            else
            {
                goal = newGoal;
            }
            h = goal.Count;
            // f(node) = g(node) + h(node)
            cost = g + h * heuristicMultiplier;

            // additionally calculate the goal without any world effect to understand if we are done
            var diff = ReGoapState<T, W>.Instantiate();
            goal.MissingDifference(planner.GetCurrentAgent().GetMemory().GetWorldState(), ref diff);
            goalMergedWithWorld = diff;
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

        public static ReGoapNode<T, W> Instantiate(IGoapPlanner<T, W> planner, ReGoapState<T, W> newGoal, ReGoapNode<T, W> parent, IReGoapAction<T, W> action, ReGoapState<T, W> actionSettings)
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
            node.Init(planner, newGoal, parent, action, actionSettings);
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

            GoapActionStackData<T, W> stackData;
            stackData.currentState = state;
            stackData.goalState = goal;
            stackData.next = action;
            stackData.agent = agent;
            stackData.settings = null;
            for (var index = actions.Count - 1; index >= 0; index--)
            {
                var possibleAction = actions[index];

                possibleAction.Precalculations(stackData);
                var settingsList = possibleAction.GetSettings(stackData);
                foreach (var settings in settingsList)
                {
                    stackData.settings = settings;
                    var precond = possibleAction.GetPreconditions(stackData);
                    var effects = possibleAction.GetEffects(stackData);

                    if (effects.HasAny(goal) && // any effect is the current goal
                        !goal.HasAnyConflict(effects, precond) && // no precondition is conflicting with the goal or has conflict but the effects fulfils the goal
                        !goal.HasAnyConflict(effects) && // no effect is conflicting with the goal
                        possibleAction.CheckProceduralCondition(stackData))
                    {
                        var newGoal = goal;
                        expandList.Add(Instantiate(planner, newGoal, this, possibleAction, settings));
                    }
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
            return goalMergedWithWorld.Count <= 0;
        }

        public float Priority { get; set; }
        public long InsertionIndex { get; set; }
        public int QueueIndex { get; set; }
    }
}