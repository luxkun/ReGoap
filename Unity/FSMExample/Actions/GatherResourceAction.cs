using System;
using UnityEngine;
using System.Collections;

public class GatherResourceAction : GoapAction
{
    public float ResourcePerAction = 1f;
    protected ResourcesBag bag;
    protected Vector3 resourcePosition;
    protected IResource resource;

    protected override void Awake()
    {
        base.Awake();

        bag = GetComponent<ResourcesBag>();
    }

    protected virtual string GetNeededResourceFromGoal(ReGoapState goalState)
    {
        foreach (var pair in goalState.GetValues())
        {
            if (pair.Key.StartsWith("hasResource"))
            {
                return pair.Key.Substring(11);
            }
        }
        return null;
    }

    public override void Precalculations(IReGoapAgent goapAgent, ReGoapState goalState)
    {
        var newNeededResourceName = GetNeededResourceFromGoal(goalState);
        preconditions.Clear();
        effects.Clear();
        if (newNeededResourceName != null)
        {
            resource = agent.GetMemory().GetWorldState().Get<IResource>("nearest" + newNeededResourceName);
            if (resource != null)
            {
                resourcePosition =
                    agent.GetMemory()
                        .GetWorldState()
                        .Get<Vector3>(string.Format("nearest{0}Position", newNeededResourceName));
                preconditions.Set("isAtPosition", resourcePosition);
                // false preconditions are not supported
                //preconditions.Set("hasResource" + newNeededResourceName, false);
                effects.Set("hasResource" + newNeededResourceName, true);

                settings = new GatherResourceSettings
                {
                    ResourcePosition = resourcePosition,
                    Resource = resource
                };
            }
        }
        base.Precalculations(goapAgent, goalState);
    }

    public override bool CheckProceduralCondition(IReGoapAgent goapAgent, ReGoapState goalState, IReGoapAction next = null)
    {
        return base.CheckProceduralCondition(goapAgent, goalState) && bag != null;
    }

    public override void Run(IReGoapAction previous, IReGoapAction next, IReGoapActionSettings settings, ReGoapState goalState, Action<IReGoapAction> done, Action<IReGoapAction> fail)
    {
        base.Run(previous, next, settings, goalState, done, fail);
        SetNeededResources(settings);
        if (resource == null || resource.GetCapacity() < ResourcePerAction) 
            failCallback(this);
        else
        {
            ReGoapLogger.Log("[GatherResourceAction] acquired " + ResourcePerAction + " " + resource.GetName());
            resource.RemoveResource(ResourcePerAction);
            bag.AddResource(resource.GetName(), ResourcePerAction);
            doneCallback(this);
        }
    }

    private void SetNeededResources(IReGoapActionSettings settings)
    {
        var thisSettings = (GatherResourceSettings) settings;
        resourcePosition = thisSettings.ResourcePosition;
        resource = thisSettings.Resource;
    }
}

internal class GatherResourceSettings : IReGoapActionSettings
{
    public Vector3 ResourcePosition;
    public IResource Resource;
}