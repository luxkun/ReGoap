using System;
using System.Collections.Generic;
using System.Linq;

public class AStar
{
    public static INode<T> Run<T>(INode<T> start, T goal, int maxIterations = 1000, int maxNodesToExpand = 1000, bool earlyExit = true)
    {
        var frontier = new FastPriorityQueue<INode<T>, T>(maxNodesToExpand);
        var stateToNode = new Dictionary<T, INode<T>>();
        frontier.Enqueue(start, start.GetCost());
        var explored = new Dictionary<T, INode<T>>(); // State -> node
        var iterations = 0;
        while ((frontier.Count > 0) && (iterations < maxIterations) && (frontier.Count + 1 < frontier.MaxSize))
        {
            iterations++;
            var node = frontier.Dequeue();
            if (node.IsGoal(goal))
            {
                ReGoapLogger.Log("[Astar] Success iterations: " + iterations);
                return node;
            }
            explored[node.GetState()] = node;
            foreach (var child in node.Expand())
            {
                if (earlyExit && child.IsGoal(goal))
                {
                    ReGoapLogger.Log("[Astar] (early exit) Success iterations: " + iterations);
                    return child;
                }
                var childCost = child.GetCost();
                var state = child.GetState();
                if (explored.ContainsKey(state))
                    continue;
                INode<T> similiarNode;
                stateToNode.TryGetValue(state, out similiarNode);
                if (similiarNode != null)
                    if (similiarNode.GetCost() > childCost)
                        frontier.Remove(similiarNode);
                    else
                        break;
                frontier.Enqueue(child, childCost);
                stateToNode[state] = child;
            }
        }
        ReGoapLogger.LogWarning("[Astar] failed.");
        return null;
    }
}

public interface INode<T>
{
    T GetState();
    List<INode<T>> Expand();

    List<INode<T>> CalculatePath();

    int CompareTo(INode<T> other);
    int GetCost();
    int GetHeuristicCost();
    int GetPathCost();
    INode<T> GetParent();
    bool IsGoal(T goal);

    // for fastpriorityqueue
    double Priority { get; set; }
    long InsertionIndex { get; set; }
    int QueueIndex { get; set; }
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
