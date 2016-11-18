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
                Debug.Log("[Astar] Success iterations: " + iterations);
                return node;
            }
            explored[node.GetState()] = node;
            foreach (var child in node.Expand())
            {
                if (earlyExit && child.IsGoal(goal))
                {
                    Debug.Log("[Astar] (early exit) Success iterations: " + iterations);
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
        Debug.LogWarning("AStar failed.");
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

public class CheapestNodeQueue
{
    private readonly Dictionary<object, INode> stateToNode;
    private readonly SortedDictionary<int, List<INode>> costToQueue;
    private int lowestValue;

    public CheapestNodeQueue()
    {
        stateToNode = new Dictionary<object, INode>();
        costToQueue = new SortedDictionary<int, List<INode>>();
        lowestValue = int.MaxValue;
    }

    public void Enqueue(INode node)
    {
        var state = node.GetState();
        if (stateToNode.ContainsKey(state))
            throw new Exception("[AStar] Trying to enqueue an already present state.");
        stateToNode[state] = node;
        var cost = node.GetCost();
        List<INode> queue;
        if (!costToQueue.TryGetValue(cost, out queue))
        {
            queue = new List<INode> {node};
            costToQueue.Add(cost, queue);
            if (cost < lowestValue)
                lowestValue = cost;
        }
        else
        {
            queue.Add(node);
            if (cost < lowestValue)
                lowestValue = cost;
        }
    }

    public INode Dequeue()
    {
        var queue = costToQueue[lowestValue]; //.First();
        var toReturn = queue[0];
        queue.RemoveAt(0);
        CleanQueue(lowestValue);
        return toReturn;
    }

    private void CleanQueue(int cost)
    {
        var queue = costToQueue[cost];
        if (queue.Count == 0)
            if (costToQueue.Count > 0)
                lowestValue = costToQueue.First().Key;
            else
                lowestValue = int.MaxValue;
    }

    public int Count
    {
        get { return costToQueue.Count; }
        private set { }
    }

    public bool Contains(INode node)
    {
        List<INode> queue;
        if (!costToQueue.TryGetValue(node.GetCost(), out queue)) return false;
        if (queue.Contains(node)) return true;
        return false;
    }

    public INode StateToNode(object state)
    {
        INode result;
        stateToNode.TryGetValue(state, out result);
        return result;
    }

    public void Remove(INode node)
    {
        costToQueue[node.GetCost()].Remove(node);
    }
}