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

    public float minResourceValue = 1f;
    public float minPowDistanceToBeNear = 1f;
    private ResourcesBag resourcesBag;

    public override void Init(IReGoapMemory memory)
    {
        base.Init(memory);
        resourcesBag = GetComponent<ResourcesBag>();
    }

    public override void UpdateSensor()
    {
        var worldState = memory.GetWorldState();
        worldState.Set("seeTree", TreeResourceManager.instance.GetResourcesCount() >= minResourceValue);

        // since every agent will use same trees we cache this function
        if (Time.time > cacheUpdateDelay || cachedResources == null)
        {
            UpdateResources(TreeResourceManager.instance);
            cachedResources = resourcesPosition;
            cacheUpdateDelay = Time.time + cacheUpdateCooldown;
        }
        var nearestTree = Utilities.GetNearest(transform.position, cachedResources);
        worldState.Set("nearestTree", nearestTree);
        if (nearestTree != null &&
            (transform.position - nearestTree.GetTransform().position).sqrMagnitude < minPowDistanceToBeNear)
        {
            worldState.Set("isAtTransform", nearestTree.GetTransform());
        }
        else if (nearestTree != null && worldState.Get<Transform>("isAtTransform") == nearestTree.GetTransform())
        {
            worldState.Set<Transform>("isAtTransform", null);
        }
    }
}
