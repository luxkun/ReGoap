using System;
using System.Collections.Generic;
using System.Linq;

public class AStar
{
    public static INode Run(INode start, object goal, int maxIterations = 1000, bool earlyExit = true)
    {
        var frontier = new SimplePriorityQueue<INode>();
        var stateToNode = new Dictionary<object, INode>();
        frontier.Enqueue(start, start.GetCost());
        var explored = new Dictionary<object, INode>(); // State -> node
        var iterations = 0;
        while ((frontier.Count > 0) && (iterations < maxIterations))
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
                INode similiarNode;
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
        ReGoapLogger.LogWarning("AStar failed.");
        return null;
    }
}

public interface INode
{
    object GetState();
    List<INode> Expand();

    List<INode> CalculatePath();

    int CompareTo(INode other);
    int GetCost();
    int GetHeuristicCost();
    int GetPathCost();
    INode GetParent();
    bool IsGoal(object goal);
}

public class NodeComparer : IComparer<INode>
{
    public int Compare(INode x, INode y)
    {
        var result = x.CompareTo(y);
        if (result == 0)
            return 1;
        return result;
    }
}
