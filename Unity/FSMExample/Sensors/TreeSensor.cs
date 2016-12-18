using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// the agent in this example is a villager which knows the location of trees, so seeTree is always true if there is an available  tree
public class TreeSensor : ResourceSensor
{
    private static Dictionary<IResource, Vector3> cachedResources;
    private static float cacheUpdateDelay;
    private static float cacheUpdateCooldown = 1f;

    public float MinResourceValue = 1f;
    public float MinPowDistanceToBeNear = 1f;
    private ResourcesBag resourcesBag;

    public override void Init(IReGoapMemory memory)
    {
        base.Init(memory);
        resourcesBag = GetComponent<ResourcesBag>();
    }

    public override void UpdateSensor()
    {
        var worldState = memory.GetWorldState();
        worldState.Set("seeTree", TreeResourceManager.Instance.GetResourcesCount() >= MinResourceValue);

        // since every agent will use same trees we cache this function
        if (Time.time > cacheUpdateDelay || cachedResources == null)
        {
            UpdateResources(TreeResourceManager.Instance);
            cachedResources = resourcesPosition;
            cacheUpdateDelay = Time.time + cacheUpdateCooldown;
        }
        var nearestTree = Utilities.GetNearest(transform.position, cachedResources);
        worldState.Set("nearestTree", nearestTree);
        worldState.Set("nearestTreePosition", nearestTree != null ? nearestTree.GetTransform().position : Vector3.zero);
        if (nearestTree != null &&
            (transform.position - nearestTree.GetTransform().position).sqrMagnitude < MinPowDistanceToBeNear)
        {
            worldState.Set("isAtTransform", nearestTree.GetTransform());
        }
        else if (nearestTree != null && worldState.Get<Transform>("isAtTransform") == nearestTree.GetTransform())
        {
            worldState.Set<Transform>("isAtTransform", null);
        }
    }
}
