using System;
using UnityEngine;
using System.Collections;

public class ChopTreeAction : GoapAction
{
    public float resourcePerChop = 1f;
    private ResourcesBag bag;

    protected override void Awake()
    {
        base.Awake();
        preconditions.Set("isAt", "tree");
        preconditions.Set("hasLog", false);
        effects.Set("hasLog", true);

        bag = GetComponent<ResourcesBag>();
    }

    public override bool CheckProceduralCondition(IReGoapAgent goapAgent, ReGoapState goalState)
    {
        return base.CheckProceduralCondition(goapAgent, goalState) && bag != null;
    }

    public override void Run(IReGoapAction previous, IReGoapAction next, ReGoapState goalState, Action<IReGoapAction> done, Action<IReGoapAction> fail)
    {
        base.Run(previous, next, goalState, done, fail);
        var tree = agent.GetMemory().GetWorldState().Get<IResource>("nearestTree");
        // should not happen since treeSensor filters already trees with low capacity
        if (tree == null || tree.GetCapacity() < resourcePerChop) 
            failCallback(this);
        else
        {
            ReGoapLogger.Log("[ChopTreeAction] chopped " + resourcePerChop + " logs.");
            tree.RemoveResource(resourcePerChop);
            bag.AddResource(TreeResourceManager.instance.GetResourceName(), resourcePerChop);
            doneCallback(this);
        }
    }
}
