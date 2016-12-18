using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ResourcesBag))]
public class AddResourceToBankAction : GoapAction
{
    public string ResourceName;
    private ResourcesBag resourcesBag;

    protected override void Awake()
    {
        base.Awake();
        resourcesBag = GetComponent<ResourcesBag>();

        preconditions.Set("has" + ResourceName, true);
        preconditions.Set("collected" + ResourceName, false);
        effects.Set("collected" + ResourceName, true);
    }

    public override void Precalculations(IReGoapAgent goapAgent, ReGoapState goalState)
    {
        base.Precalculations(goapAgent, goalState);
        var bankPosition = agent.GetMemory().GetWorldState().Get<Vector3>("nearestBankPosition");
        if (bankPosition != default(Vector3))
            preconditions.Set("isAtPosition", bankPosition);
    }


    public override void Run(IReGoapAction previous, IReGoapAction next, ReGoapState goalState, Action<IReGoapAction> done, Action<IReGoapAction> fail)
    {
        base.Run(previous, next, goalState, done, fail);
        var bank = agent.GetMemory().GetWorldState().Get<Bank>("nearestBank");
        if (bank.AddResource(resourcesBag, ResourceName))
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
        return string.Format("GoapAction('{0}', '{1}')", Name, ResourceName);
    }
}
