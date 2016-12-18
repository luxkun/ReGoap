using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// the agent in this example is a villager which knows the location of trees, so seeTree is always true if there is an available  tree
public class MultipleResourcesSensor : ResourceSensor
{
    private static Dictionary<string, Dictionary<IResource, Vector3>> cachedResources;
    private static float cacheUpdateDelay;
    private static float cacheUpdateCooldown = 1f;

    public float MinResourceValue = 1f;
    public float MinPowDistanceToBeNear = 1f;

    public override void UpdateSensor()
    {
        var worldState = memory.GetWorldState();

        foreach (var resourceManager in MultipleResourcesManager.Instance.Resources.Values)
        {
            worldState.Set("see" + resourceManager.GetResourceName(), resourceManager.GetResourcesCount() >= MinResourceValue);

            if (cachedResources == null)
                cachedResources = new Dictionary<string, Dictionary<IResource, Vector3>>();
            // since every agent will use same resources we cache this function
            if (Time.time > cacheUpdateDelay || !cachedResources.ContainsKey(resourceManager.GetResourceName()))
            {
                UpdateResources(resourceManager);
                cachedResources[resourceManager.GetResourceName()] = resourcesPosition;
                cacheUpdateDelay = Time.time + cacheUpdateCooldown;
            }
            var nearestResource = Utilities.GetNearest(transform.position, cachedResources[resourceManager.GetResourceName()]);
            worldState.Set("nearest" + resourceManager.GetResourceName(), nearestResource);
            worldState.Set("nearest" + resourceManager.GetResourceName() + "Position",
                nearestResource != null ? nearestResource.GetTransform().position : Vector3.zero);
        }
    }
}
