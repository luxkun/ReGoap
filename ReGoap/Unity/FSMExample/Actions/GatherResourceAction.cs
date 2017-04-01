using System;
using UnityEngine;
using System.Collections;

public class GatherResourceAction : ReGoapAction<string, object>
{
    public float TimeToGather = 0.5f;
    public float ResourcePerAction = 1f;
    protected ResourcesBag bag;
    protected Vector3? resourcePosition;
    protected IResource resource;

    private float gatherCooldown;

    protected override void Awake()
    {
        base.Awake();

        bag = GetComponent<ResourcesBag>();
    }

    protected virtual string GetNeededResourceFromGoal(ReGoapState<string, object> goalState)
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

    public override ReGoapState<string, object> GetPreconditions(ReGoapState<string, object> goalState, IReGoapAction<string, object> next = null)
    {
        var newNeededResourceName = GetNeededResourceFromGoal(goalState);
        preconditions.Clear();
        if (newNeededResourceName != null)
        {
            resource = agent.GetMemory().GetWorldState().Get("nearest" + newNeededResourceName) as IResource;
            if (resource != null)
            {
                resourcePosition = agent.GetMemory().GetWorldState()
                    .Get(string.Format("nearest{0}Position", newNeededResourceName)) as Vector3?;
                preconditions.Set("isAtPosition", resourcePosition);
            }
        }
        return preconditions;
    }

    public override ReGoapState<string, object> GetEffects(ReGoapState<string, object> goalState, IReGoapAction<string, object> next = null)
    {
        var newNeededResourceName = GetNeededResourceFromGoal(goalState);
        effects.Clear();
        if (newNeededResourceName != null)
        {
            resource = agent.GetMemory().GetWorldState().Get("nearest" + newNeededResourceName) as IResource;
            if (resource != null)
            {
                resourcePosition = agent.GetMemory().GetWorldState().Get(string.Format("nearest{0}Position", newNeededResourceName)) as Vector3?;
                effects.Set("hasResource" + newNeededResourceName, true);
            }
        }
        return effects;
    }

    public override IReGoapActionSettings<string, object> GetSettings(IReGoapAgent<string, object> goapAgent, ReGoapState<string, object> goalState)
    {
        var newNeededResourceName = GetNeededResourceFromGoal(goalState);
        settings = null;
        if (newNeededResourceName != null)
        {
            resource = agent.GetMemory().GetWorldState().Get("nearest" + newNeededResourceName) as IResource;
            if (resource != null)
            {
                resourcePosition = (Vector3) agent.GetMemory().GetWorldState()
                    .Get(string.Format("nearest{0}Position", newNeededResourceName));

                settings = new GatherResourceSettings
                {
                    ResourcePosition = resourcePosition,
                    Resource = resource
                };
            }
        }
        return settings;
    }

    public override bool CheckProceduralCondition(IReGoapAgent<string, object> goapAgent, ReGoapState<string, object> goalState, IReGoapAction<string, object> next = null)
    {
        return base.CheckProceduralCondition(goapAgent, goalState) && bag != null;
    }

    public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, IReGoapActionSettings<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
    {
        base.Run(previous, next, settings, goalState, done, fail);
        SetNeededResources(settings);
        if (resource == null || resource.GetCapacity() < ResourcePerAction)
            failCallback(this);
        else
        {
            gatherCooldown = Time.time + TimeToGather;
        }
    }

    private void SetNeededResources(IReGoapActionSettings<string, object> settings)
    {
        var thisSettings = (GatherResourceSettings)settings;
        resourcePosition = thisSettings.ResourcePosition;
        resource = thisSettings.Resource;
    }

    protected override void Update()
    {
        base.Update();

        if (resource == null || resource.GetCapacity() < ResourcePerAction)
            failCallback(this);
        else if (Time.time > gatherCooldown)
        {
            gatherCooldown = float.MaxValue;
            ReGoapLogger.Log("[GatherResourceAction] acquired " + ResourcePerAction + " " + resource.GetName());
            resource.RemoveResource(ResourcePerAction);
            bag.AddResource(resource.GetName(), ResourcePerAction);
            doneCallback(this);
        }
    }
}

internal class GatherResourceSettings : IReGoapActionSettings<string, object>
{
    public Vector3? ResourcePosition;
    public IResource Resource;
}