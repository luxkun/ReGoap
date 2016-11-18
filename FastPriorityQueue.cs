using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The IPriorityQueue interface.  This is mainly here for purists, and in case I decide to add more implementations later.
/// For speed purposes, it is actually recommended that you *don't* access the priority queue through this interface, since the JIT can
/// (theoretically?) optimize method calls from concrete-types slightly better.
/// </summary>
public interface IPriorityQueue<T> : IEnumerable<T>
{
    /// <summary>
    /// Enqueue a node to the priority queue.  Lower values are placed in front. Ties are broken by first-in-first-out.
    /// See implementation for how duplicates are handled.
    /// </summary>
    void Enqueue(T node, double priority);

    /// <summary>
    /// Removes the head of the queue (node with minimum priority; ties are broken by order of insertion), and returns it.
    /// </summary>
    T Dequeue();

    /// <summary>
    /// Removes every node from the queue.
    /// </summary>
    void Clear();

    /// <summary>
    /// Returns whether the given node is in the queue.
    /// </summary>
    bool Contains(T node);

    /// <summary>
    /// Removes a node from the queue.  The node does not need to be the head of the queue.  
    /// </summary>
    void Remove(T node);

    /// <summary>
    /// Call this method to change the priority of a node.  
    /// </summary>
    void UpdatePriority(T node, double priority);

    /// <summary>
    /// Returns the head of the queue, without removing it (use Dequeue() for that).
    /// </summary>
    T First { get; }

    /// <summary>
    /// Returns the number of nodes in the queue.
    /// </summary>
    int Count { get; }
}

public class FastPriorityQueueNode
{
    /// <summary>
    /// The Priority to insert this node at.  Must be set BEFORE adding a node to the queue
    /// </summary>
    public double Priority { get; set; }

    /// <summary>
    /// <b>Used by the priority queue - do not edit this value.</b>
    /// Represents the order the node was inserted in
    /// </summary>
    public long InsertionIndex { get; set; }

    /// <summary>
    /// <b>Used by the priority queue - do not edit this value.</b>
    /// Represents the current position in the queue
    /// </summary>
    public int QueueIndex { get; set; }
}

/// <summary>
/// An implementation of a min-Priority Queue using a heap.  Has O(1) .Contains()!
/// See https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp/wiki/Getting-Started for more information
/// </summary>
/// <typeparam name="T">The values in the queue.  Must extend the FastPriorityQueueNode class</typeparam>
public sealed class FastPriorityQueue<T> : IPriorityQueue<T>
    where T : FastPriorityQueueNode
{
    private int numNodes;
    private T[] nodes;
    private long numNodesEverEnqueued;

    /// <summary>
    /// Instantiate a new Priority Queue
    /// </summary>
    /// <param name="maxNodes">The max nodes ever allowed to be enqueued (going over this will cause undefined behavior)</param>
    public FastPriorityQueue(int maxNodes)
    {
#if VERBOSE
        if (maxNodes <= 0)
        {
            throw new InvalidOperationException("New queue size cannot be smaller than 1");
        }
#endif

        numNodes = 0;
        nodes = new T[maxNodes + 1];
        numNodesEverEnqueued = 0;
    }

    /// <summary>
    /// Returns the number of nodes in the queue.
    /// O(1)
    /// </summary>
    public int Count
    {
        get { return numNodes; }
    }

    /// <summary>
    /// Returns the maximum number of items that can be enqueued at once in this queue.  Once you hit this number (ie. once Count == MaxSize),
    /// attempting to enqueue another item will cause undefined behavior.  O(1)
    /// </summary>
    public int MaxSize
    {
        get { return nodes.Length - 1; }
    }

    /// <summary>
    /// Removes every node from the queue.
    /// O(n) (So, don't do this often!)
    /// </summary>
#if NET_VERSION_4_5
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    #endif
    public void Clear()
    {
        Array.Clear(nodes, 1, numNodes);
        numNodes = 0;
    }

    /// <summary>
    /// Returns (in O(1)!) whether the given node is in the queue.  O(1)
    /// </summary>
#if NET_VERSION_4_5
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    #endif
    public bool Contains(T node)
    {
#if VERBOSE
        if(node == null)
        {
            throw new ArgumentNullException("node");
        }
        if(node.QueueIndex < 0 || node.QueueIndex >= _nodes.Length)
        {
            throw new InvalidOperationException("node.QueueIndex has been corrupted. Did you change it manually? Or add this node to another queue?");
        }
#endif

        return nodes[node.QueueIndex] == node;
    }

    /// <summary>
    /// Enqueue a node to the priority queue.  Lower values are placed in front. Ties are broken by first-in-first-out.
    /// If the queue is full, the result is undefined.
    /// If the node is already enqueued, the result is undefined.
    /// O(log n)
    /// </summary>
#if NET_VERSION_4_5
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    #endif
    public void Enqueue(T node, double priority)
    {
#if VERBOSE
        if(node == null)
        {
            throw new ArgumentNullException("node");
        }
        if(_numNodes >= _nodes.Length - 1)
        {
            throw new InvalidOperationException("Queue is full - node cannot be added: " + node);
        }
        if(Contains(node))
        {
            throw new InvalidOperationException("Node is already enqueued: " + node);
        }
#endif

        node.Priority = priority;
        numNodes++;
        nodes[numNodes] = node;
        node.QueueIndex = numNodes;
        node.InsertionIndex = numNodesEverEnqueued++;
        CascadeUp(nodes[numNodes]);
    }

#if NET_VERSION_4_5
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    #endif

    private void Swap(T node1, T node2)
    {
        //Swap the nodes
        nodes[node1.QueueIndex] = node2;
        nodes[node2.QueueIndex] = node1;

        //Swap their indicies
        var temp = node1.QueueIndex;
        node1.QueueIndex = node2.QueueIndex;
        node2.QueueIndex = temp;
    }

    //Performance appears to be slightly better when this is NOT inlined o_O
    private void CascadeUp(T node)
    {
        //aka Heapify-up
        var parent = node.QueueIndex/2;
        while (parent >= 1)
        {
            var parentNode = nodes[parent];
            if (HasHigherPriority(parentNode, node))
                break;

            //Node has lower priority value, so move it up the heap
            Swap(node, parentNode);
                //For some reason, this is faster with Swap() rather than (less..?) individual operations, like in CascadeDown()

            parent = node.QueueIndex/2;
        }
    }

#if NET_VERSION_4_5
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    #endif

    private void CascadeDown(T node)
    {
        //aka Heapify-down
        T newParent;
        var finalQueueIndex = node.QueueIndex;
        while (true)
        {
            newParent = node;
            var childLeftIndex = 2*finalQueueIndex;

            //Check if the left-child is higher-priority than the current node
            if (childLeftIndex > numNodes)
            {
                //This could be placed outside the loop, but then we'd have to check newParent != node twice
                node.QueueIndex = finalQueueIndex;
                nodes[finalQueueIndex] = node;
                break;
            }

            var childLeft = nodes[childLeftIndex];
            if (HasHigherPriority(childLeft, newParent))
                newParent = childLeft;

            //Check if the right-child is higher-priority than either the current node or the left child
            var childRightIndex = childLeftIndex + 1;
            if (childRightIndex <= numNodes)
            {
                var childRight = nodes[childRightIndex];
                if (HasHigherPriority(childRight, newParent))
                    newParent = childRight;
            }

            //If either of the children has higher (smaller) priority, swap and continue cascading
            if (newParent != node)
            {
                //Move new parent to its new index.  node will be moved once, at the end
                //Doing it this way is one less assignment operation than calling Swap()
                nodes[finalQueueIndex] = newParent;

                var temp = newParent.QueueIndex;
                newParent.QueueIndex = finalQueueIndex;
                finalQueueIndex = temp;
            }
            else
            {
                //See note above
                node.QueueIndex = finalQueueIndex;
                nodes[finalQueueIndex] = node;
                break;
            }
        }
    }

    /// <summary>
    /// Returns true if 'higher' has higher priority than 'lower', false otherwise.
    /// Note that calling HasHigherPriority(node, node) (ie. both arguments the same node) will return false
    /// </summary>
#if NET_VERSION_4_5
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    #endif
    private bool HasHigherPriority(T higher, T lower)
    {
        return (higher.Priority < lower.Priority) ||
               ((higher.Priority == lower.Priority) && (higher.InsertionIndex < lower.InsertionIndex));
    }

    /// <summary>
    /// Removes the head of the queue (node with minimum priority; ties are broken by order of insertion), and returns it.
    /// If queue is empty, result is undefined
    /// O(log n)
    /// </summary>
    public T Dequeue()
    {
#if VERBOSE
        if(_numNodes <= 0)
        {
            throw new InvalidOperationException("Cannot call Dequeue() on an empty queue");
        }

        if(!IsValidQueue())
        {
            throw new InvalidOperationException("Queue has been corrupted (Did you update a node priority manually instead of calling UpdatePriority()?" +
                                                "Or add the same node to two different queues?)");
        }
#endif

        var returnMe = nodes[1];
        Remove(returnMe);
        return returnMe;
    }

    /// <summary>
    /// Resize the queue so it can accept more nodes.  All currently enqueued nodes are remain.
    /// Attempting to decrease the queue size to a size too small to hold the existing nodes results in undefined behavior
    /// O(n)
    /// </summary>
    public void Resize(int maxNodes)
    {
#if VERBOSE
        if (maxNodes <= 0)
        {
            throw new InvalidOperationException("Queue size cannot be smaller than 1");
        }

        if (maxNodes < _numNodes)
        {
            throw new InvalidOperationException("Called Resize(" + maxNodes + "), but current queue contains " + _numNodes + " nodes");
        }
#endif

        var newArray = new T[maxNodes + 1];
        var highestIndexToCopy = Math.Min(maxNodes, numNodes);
        for (var i = 1; i <= highestIndexToCopy; i++)
            newArray[i] = nodes[i];
        nodes = newArray;
    }

    /// <summary>
    /// Returns the head of the queue, without removing it (use Dequeue() for that).
    /// If the queue is empty, behavior is undefined.
    /// O(1)
    /// </summary>
    public T First
    {
        get
        {
#if VERBOSE
            if(_numNodes <= 0)
            {
                throw new InvalidOperationException("Cannot call .First on an empty queue");
            }
#endif

            return nodes[1];
        }
    }

    /// <summary>
    /// This method must be called on a node every time its priority changes while it is in the queue.  
    /// <b>Forgetting to call this method will result in a corrupted queue!</b>
    /// Calling this method on a node not in the queue results in undefined behavior
    /// O(log n)
    /// </summary>
#if NET_VERSION_4_5
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    #endif
    public void UpdatePriority(T node, double priority)
    {
#if VERBOSE
        if(node == null)
        {
            throw new ArgumentNullException("node");
        }
        if(!Contains(node))
        {
            throw new InvalidOperationException("Cannot call UpdatePriority() on a node which is not enqueued: " + node);
        }
#endif

        node.Priority = priority;
        OnNodeUpdated(node);
    }

    private void OnNodeUpdated(T node)
    {
        //Bubble the updated node up or down as appropriate
        var parentIndex = node.QueueIndex/2;
        var parentNode = nodes[parentIndex];

        if ((parentIndex > 0) && HasHigherPriority(node, parentNode))
            CascadeUp(node);
        else
            CascadeDown(node);
    }

    /// <summary>
    /// Removes a node from the queue.  The node does not need to be the head of the queue.  
    /// If the node is not in the queue, the result is undefined.  If unsure, check Contains() first
    /// O(log n)
    /// </summary>
    public void Remove(T node)
    {
#if VERBOSE
        if(node == null)
        {
            throw new ArgumentNullException("node");
        }
        if(!Contains(node))
        {
            throw new InvalidOperationException("Cannot call Remove() on a node which is not enqueued: " + node);
        }
#endif

        //If the node is already the last node, we can remove it immediately
        if (node.QueueIndex == numNodes)
        {
            nodes[numNodes] = null;
            numNodes--;
            return;
        }

        //Swap the node with the last node
        var formerLastNode = nodes[numNodes];
        Swap(node, formerLastNode);
        nodes[numNodes] = null;
        numNodes--;

        //Now bubble formerLastNode (which is no longer the last node) up or down as appropriate
        OnNodeUpdated(formerLastNode);
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 1; i <= numNodes; i++)
            yield return nodes[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// <b>Should not be called in production code.</b>
    /// Checks to make sure the queue is still in a valid state.  Used for testing/debugging the queue.
    /// </summary>
    public bool IsValidQueue()
    {
        for (var i = 1; i < nodes.Length; i++)
            if (nodes[i] != null)
            {
                var childLeftIndex = 2*i;
                if ((childLeftIndex < nodes.Length) && (nodes[childLeftIndex] != null) &&
                    HasHigherPriority(nodes[childLeftIndex], nodes[i]))
                    return false;

                var childRightIndex = childLeftIndex + 1;
                if ((childRightIndex < nodes.Length) && (nodes[childRightIndex] != null) &&
                    HasHigherPriority(nodes[childRightIndex], nodes[i]))
                    return false;
            }
        return true;
    }
}

public sealed class SimplePriorityQueue<T> : IPriorityQueue<T>
{
    private class SimpleNode : FastPriorityQueueNode
    {
        public T Data { get; private set; }

        public SimpleNode(T data)
        {
            Data = data;
        }
    }

    private const int InitialQueueSize = 10;
    private readonly FastPriorityQueue<SimpleNode> queue;

    public SimplePriorityQueue()
    {
        queue = new FastPriorityQueue<SimpleNode>(InitialQueueSize);
    }

    /// <summary>
    /// Given an item of type T, returns the exist SimpleNode in the queue
    /// </summary>
    private SimpleNode GetExistingNode(T item)
    {
        var comparer = EqualityComparer<T>.Default;
        foreach (var node in queue)
            if (comparer.Equals(node.Data, item))
                return node;
        throw new InvalidOperationException("Item cannot be found in queue: " + item);
    }

    /// <summary>
    /// Returns the number of nodes in the queue.
    /// O(1)
    /// </summary>
    public int Count
    {
        get
        {
            lock (queue)
            {
                return queue.Count;
            }
        }
    }


    /// <summary>
    /// Returns the head of the queue, without removing it (use Dequeue() for that).
    /// Throws an exception when the queue is empty.
    /// O(1)
    /// </summary>
    public T First
    {
        get
        {
            lock (queue)
            {
                if (queue.Count <= 0)
                    throw new InvalidOperationException("Cannot call .First on an empty queue");

                var first = queue.First;
                return first != null ? first.Data : default(T);
            }
        }
    }

    /// <summary>
    /// Removes every node from the queue.
    /// O(n)
    /// </summary>
    public void Clear()
    {
        lock (queue)
        {
            queue.Clear();
        }
    }

    /// <summary>
    /// Returns whether the given item is in the queue.
    /// O(n)
    /// </summary>
    public bool Contains(T item)
    {
        lock (queue)
        {
            var comparer = EqualityComparer<T>.Default;
            foreach (var node in queue)
                if (comparer.Equals(node.Data, item))
                    return true;
            return false;
        }
    }

    /// <summary>
    /// Removes the head of the queue (node with minimum priority; ties are broken by order of insertion), and returns it.
    /// If queue is empty, throws an exception
    /// O(log n)
    /// </summary>
    public T Dequeue()
    {
        lock (queue)
        {
            if (queue.Count <= 0)
                throw new InvalidOperationException("Cannot call Dequeue() on an empty queue");

            var node = queue.Dequeue();
            return node.Data;
        }
    }

    /// <summary>
    /// Enqueue a node to the priority queue.  Lower values are placed in front. Ties are broken by first-in-first-out.
    /// This queue automatically resizes itself, so there's no concern of the queue becoming 'full'.
    /// Duplicates are allowed.
    /// O(log n)
    /// </summary>
    public void Enqueue(T item, double priority)
    {
        lock (queue)
        {
            var node = new SimpleNode(item);
            if (queue.Count == queue.MaxSize)
                queue.Resize(queue.MaxSize*2 + 1);
            queue.Enqueue(node, priority);
        }
    }

    /// <summary>
    /// Removes an item from the queue.  The item does not need to be the head of the queue.  
    /// If the item is not in the queue, an exception is thrown.  If unsure, check Contains() first.
    /// If multiple copies of the item are enqueued, only the first one is removed. 
    /// O(n)
    /// </summary>
    public void Remove(T item)
    {
        lock (queue)
        {
            try
            {
                queue.Remove(GetExistingNode(item));
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Cannot call Remove() on a node which is not enqueued: " + item, ex);
            }
        }
    }

    /// <summary>
    /// Call this method to change the priority of an item.
    /// Calling this method on a item not in the queue will throw an exception.
    /// If the item is enqueued multiple times, only the first one will be updated.
    /// (If your requirements are complex enough that you need to enqueue the same item multiple times <i>and</i> be able
    /// to update all of them, please wrap your items in a wrapper class so they can be distinguished).
    /// O(n)
    /// </summary>
    public void UpdatePriority(T item, double priority)
    {
        lock (queue)
        {
            try
            {
                var updateMe = GetExistingNode(item);
                queue.UpdatePriority(updateMe, priority);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(
                    "Cannot call UpdatePriority() on a node which is not enqueued: " + item, ex);
            }
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        var queueData = new List<T>();
        lock (queue)
        {
            //Copy to a separate list because we don't want to 'yield return' inside a lock
            foreach (var node in queue)
                queueData.Add(node.Data);
        }

        return queueData.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool IsValidQueue()
    {
        lock (queue)
        {
            return queue.IsValidQueue();
        }
    }
}