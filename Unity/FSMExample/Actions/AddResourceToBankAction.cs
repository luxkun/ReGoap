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
    }

    public override IReGoapActionSettings GetSettings(IReGoapAgent goapAgent, ReGoapState goalState)
    {
        settings = null;
        foreach (var pair in goalState.GetValues())
        {
            if (pair.Key.StartsWith("collectedResource"))
            {
                var resourceName = pair.Key.Substring(17);
                settings = new AddResourceToBankSettings
                {
                    ResourceName = resourceName
                };
                break;
            }
        }
        return settings;
    }

    public override ReGoapState GetEffects(ReGoapState goalState, IReGoapAction next = null)
    {
        effects.Clear();
        effects.Set("isAtPosition", Vector3.zero);

        foreach (var pair in goalState.GetValues())
        {
            if (pair.Key.StartsWith("collectedResource"))
            {
                var resourceName = pair.Key.Substring(17);
                effects.Set("collectedResource" + resourceName, true);
                break;
            }
        }

        return effects;
    }

    public override ReGoapState GetPreconditions(ReGoapState goalState, IReGoapAction next = null)
    {
        var bankPosition = agent.GetMemory().GetWorldState().Get<Vector3>("nearestBankPosition");

        preconditions.Clear();
        preconditions.Set("isAtPosition", bankPosition);

        foreach (var pair in goalState.GetValues())
        {
            if (pair.Key.StartsWith("collectedResource"))
            {
                var resourceName = pair.Key.Substring(17);
                preconditions.Set("hasResource" + resourceName, true);
                break;
            }
        }

        return preconditions;
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
