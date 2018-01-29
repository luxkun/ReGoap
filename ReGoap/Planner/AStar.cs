using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Utilities;

namespace ReGoap.Planner
{
    public class AStar<T>
    {
        private readonly FastPriorityQueue<INode<T>, T> frontier;
        private readonly Dictionary<T, INode<T>> stateToNode;
        private readonly Dictionary<T, INode<T>> explored;
        private readonly List<INode<T>> createdNodes;

        public AStar(int maxNodesToExpand = 1000)
        {
            frontier = new FastPriorityQueue<INode<T>, T>(maxNodesToExpand);
            stateToNode = new Dictionary<T, INode<T>>();
            explored = new Dictionary<T, INode<T>>(); // State -> node
            createdNodes = new List<INode<T>>(maxNodesToExpand);
        }

        void ClearNodes()
        {
            foreach (var node in createdNodes)
            {
                node.Recycle();
            }
            createdNodes.Clear();
        }

        private bool _debugPlan = false;
        private Assets.ReGoap.Planner.PlanDebugger _debugger;

        private void _DebugPlan(INode<T> node, INode<T> parent)
        {
            if (!_debugPlan) return;
            if (null == _debugger)
                _debugger = new Assets.ReGoap.Planner.PlanDebugger();

            string nodeStr = string.Format("{0} [label=\"GOAL({4}): {5}:\n{1}\nEFFECT:\n{2}\nPRECOND:\n{3}\n\"]", node.GetHashCode(), node.GoalString, node.EffectString, node.PrecondString, node.GetCost(), node.Name);
            _debugger.AddNode(nodeStr);

            if (parent != null)
            {
                string connStr = string.Format("{0} -> {1}", parent.GetHashCode(), node.GetHashCode());
                _debugger.AddConn(connStr);
            }
        }

        private void _EndDebugPlan(INode<T> node)
        {
            if (null != _debugger)
            {
                while (node != null)
                { //mark success path
                    string nodeStr = string.Format("{0} [style=filled, color=\"#00FF00\"]", node.GetHashCode());
                    _debugger.AddNode(nodeStr);
                    node = node.GetParent();
                }

                var txt = _debugger.TransformText();
                System.IO.Directory.CreateDirectory("PlanDebugger");
                System.IO.File.WriteAllText(string.Format("PlanDebugger/DebugPlan_{0}.dot", System.DateTime.Now.ToString("HHmmss_ffff")), txt);
                _debugger.Clear();
            }
        }

        public INode<T> Run(INode<T> start, T goal, int maxIterations = 100, bool earlyExit = true, bool clearNodes = true, bool debugPlan = false)
        {
            _debugPlan = debugPlan;

            frontier.Clear();
            stateToNode.Clear();
            explored.Clear();
            if (clearNodes)
            {
                ClearNodes();
                createdNodes.Add(start);
            }

            frontier.Enqueue(start, start.GetCost());

            _DebugPlan(start, null);

            var iterations = 0;
            while ((frontier.Count > 0) && (iterations < maxIterations) && (frontier.Count + 1 < frontier.MaxSize))
            {
                var node = frontier.Dequeue();
                //Utilities.ReGoapLogger.Log(string.Format("\n++++Explored action: {0}({3}), state ({1})\n goal ({2})\n effect: ({4})", node.Name, node.GetState(), node.GoalString, node.GetCost(), node.EffectString));
                if (node.IsGoal(goal))
                {
                    ReGoapLogger.Log("[Astar] Success iterations: " + iterations);
                    _EndDebugPlan(node);
                    return node;
                }
                explored[node.GetState()] = node;


                foreach (var child in node.Expand())
                {
                    iterations++;
                    if (clearNodes)
                    {
                        createdNodes.Add(child);
                    }
                    if (earlyExit && child.IsGoal(goal))
                    {
                        ReGoapLogger.Log("[Astar] (early exit) Success iterations: " + iterations);
                        _EndDebugPlan(child);
                        return child;
                    }
                    var childCost = child.GetCost();
                    var state = child.GetState();
                    if (explored.ContainsKey(state))
                        continue;
                    INode<T> similiarNode;
                    stateToNode.TryGetValue(state, out similiarNode);
                    if (similiarNode != null)
                    {
                        if (similiarNode.GetCost() > childCost)
                            frontier.Remove(similiarNode);
                        else
                            break;
                    }

                    _DebugPlan(child, node);

                    //Utilities.ReGoapLogger.Log(string.Format("    Enqueue frontier: {0}, cost: {1}", child.Name, childCost));
                    frontier.Enqueue(child, childCost);
                    stateToNode[state] = child;
                }
            }
            ReGoapLogger.LogWarning("[Astar] failed.");
            _EndDebugPlan(null);
            return null;
        }
    }

    public interface INode<T>
    {
        T GetState();
        List<INode<T>> Expand();
        int CompareTo(INode<T> other);
        float GetCost();
        float GetHeuristicCost();
        float GetPathCost();
        INode<T> GetParent();
        bool IsGoal(T goal);
        //int AstarID { get; } //used for planDebug
        string Name { get; }
        string GoalString { get; }
        string EffectString { get; }
        string PrecondString { get; }

        int QueueIndex { get; set; }
        float Priority { get; set; }
        void Recycle();
    }

    public class NodeComparer<T> : IComparer<INode<T>>
    {
        public int Compare(INode<T> x, INode<T> y)
        {
            var result = x.CompareTo(y);
            if (result == 0)
                return 1;
            return result;
        }
    }
}