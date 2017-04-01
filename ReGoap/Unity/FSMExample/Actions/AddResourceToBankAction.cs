using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ResourcesBag))]
public class AddResourceToBankAction : ReGoapAction<string, object>
{
    private ResourcesBag resourcesBag;

    protected override void Awake()
    {
        base.Awake();
        resourcesBag = GetComponent<ResourcesBag>();
    }

    public override IReGoapActionSettings<string, object> GetSettings(IReGoapAgent<string, object> goapAgent, ReGoapState<string, object> goalState)
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

    public override ReGoapState<string, object> GetEffects(ReGoapState<string, object> goalState, IReGoapAction<string, object> next = null)
    {
        effects.Clear();
        effects.Set("isAtPosition", (Vector3?) Vector3.zero);

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

    public override ReGoapState<string, object> GetPreconditions(ReGoapState<string, object> goalState, IReGoapAction<string, object> next = null)
    {
        var bankPosition = agent.GetMemory().GetWorldState().Get("nearestBankPosition") as Vector3?;

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


    public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, IReGoapActionSettings<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
    {
        base.Run(previous, next, settings, goalState, done, fail);
        this.settings = (AddResourceToBankSettings) settings;
        var bank = agent.GetMemory().GetWorldState().Get("nearestBank") as Bank;
        if (bank != null && bank.AddResource(resourcesBag, ((AddResourceToBankSettings) settings).ResourceName))
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

public class AddResourceToBankSettings : IReGoapActionSettings<string, object>
{
    public string ResourceName;
}
