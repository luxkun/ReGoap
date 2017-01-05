using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ResourcesBag))]
public class AddResourceToBankAction : GoapAction
{
    private ResourcesBag resourcesBag;

    protected override void Awake()
    {
        base.Awake();
        resourcesBag = GetComponent<ResourcesBag>();
    }

    public override void Precalculations(IReGoapAgent goapAgent, ReGoapState goalState)
    {
        base.Precalculations(goapAgent, goalState);
        var bankPosition = agent.GetMemory().GetWorldState().Get<Vector3>("nearestBankPosition");

        preconditions.Clear();
        effects.Clear();
        preconditions.Set("isAtPosition", bankPosition);
        effects.Set("isAtPosition", Vector3.zero);

        foreach (var pair in goalState.GetValues())
        {
            if (pair.Key.StartsWith("collectedResource"))
            {
                var resourceName = pair.Key.Substring(17);
                preconditions.Set("hasResource" + resourceName, true);
                // false preconditions are not supported
                //preconditions.Set("collectedResource" + resourceName, false);
                effects.Set("collectedResource" + resourceName, true);
                settings = new AddResourceToBankSettings
                {
                    ResourceName = resourceName
                };
                break;
            }
        }
    }


    public override void Run(IReGoapAction previous, IReGoapAction next, IReGoapActionSettings settings, ReGoapState goalState, Action<IReGoapAction> done, Action<IReGoapAction> fail)
    {
        base.Run(previous, next, settings, goalState, done, fail);
        this.settings = (AddResourceToBankSettings) settings;
        var bank = agent.GetMemory().GetWorldState().Get<Bank>("nearestBank");
        if (bank.AddResource(resourcesBag, ((AddResourceToBankSettings) settings).ResourceName))
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
        return string.Format("GoapAction('{0}')", Name);
    }
}

public class AddResourceToBankSettings : IReGoapActionSettings
{
    public string ResourceName;
}
