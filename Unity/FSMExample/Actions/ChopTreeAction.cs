using System;
using UnityEngine;
using System.Collections;

public class ChopTreeAction : GoapAction
{
    public float ResourcePerChop = 1f;
    private ResourcesBag bag;

    protected override void Awake()
    {
        base.Awake();
        preconditions.Set("hasLog", false);
        effects.Set("hasLog", true);

        bag = GetComponent<ResourcesBag>();
    }

    public override void Precalculations(IReGoapAgent goapAgent, ReGoapState goalState)
    {
        base.Precalculations(goapAgent, goalState);
        var treePosition = agent.GetMemory().GetWorldState().Get<Vector3>("nearestTreePosition");
        if (treePosition != default(Vector3))
        {
            preconditions.Set("isAtPosition", treePosition);
        }
    }

    public override bool CheckProceduralCondition(IReGoapAgent goapAgent, ReGoapState goalState, IReGoapAction next = null)
    {
        return base.CheckProceduralCondition(goapAgent, goalState) && bag != null;
    }

    public override void Run(IReGoapAction previous, IReGoapAction next, ReGoapState goalState, Action<IReGoapAction> done, Action<IReGoapAction> fail)
    {
        base.Run(previous, next, goalState, done, fail);
        // should not happen since treeSensor filters already trees with low capacity
        var tree = agent.GetMemory().GetWorldState().Get<IResource>("nearestTree");
        if (tree == null || tree.GetCapacity() < ResourcePerChop) 
            failCallback(this);
        else
        {
            ReGoapLogger.Log("[ChopTreeAction] chopped " + ResourcePerChop + " logs.");
            tree.RemoveResource(ResourcePerChop);
            bag.AddResource(TreeResourceManager.Instance.GetResourceName(), ResourcePerChop);
            doneCallback(this);
        }
    }
}
