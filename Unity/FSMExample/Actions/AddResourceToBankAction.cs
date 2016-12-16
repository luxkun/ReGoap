using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ResourcesBag))]
public class AddResourceToBankAction : GoapAction
{
    public string resourceName;
    private ResourcesBag resourcesBag;

    protected override void Awake()
    {
        base.Awake();
        resourcesBag = GetComponent<ResourcesBag>();

        preconditions.Set("has" + resourceName, true);
        preconditions.Set("collected" + resourceName, false);
        effects.Set("collected" + resourceName, true);
    }

    public override void Precalculations(IReGoapAgent goapAgent, ReGoapState goalState)
    {
        base.Precalculations(goapAgent, goalState);
        var bank = GetNearestBank();
        if (bank != null)
            preconditions.Set("isAtTransform", bank.transform);
    }

    private Bank GetNearestBank()
    {
        return agent.GetMemory().GetWorldState().Get<Bank>("nearestBank");
    }

    public override void Run(IReGoapAction previous, IReGoapAction next, ReGoapState goalState, Action<IReGoapAction> done, Action<IReGoapAction> fail)
    {
        base.Run(previous, next, goalState, done, fail);
        var bank = GetNearestBank();
        if (bank.AddResource(resourcesBag, resourceName))
        {
            done(this);
        }
        else
        {
            fail(this);
        }
    }

    public override string ToString()
    {
        return string.Format("GoapAction('{0}', '{1}')", Name, resourceName);
    }
}
