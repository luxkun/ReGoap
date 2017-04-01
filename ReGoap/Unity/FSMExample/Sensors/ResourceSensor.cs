using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// the agent in this example is a villager which knows the location of trees, so seeTree is always true if there is an available  tree
public class ResourceSensor : ReGoapSensor<string, object>
{
    protected Dictionary<IResource, Vector3> resourcesPosition;

    protected virtual void Awake()
    {
        resourcesPosition = new Dictionary<IResource, Vector3>();
    }

    protected virtual void UpdateResources(IResourceManager manager)
    {
        resourcesPosition.Clear();
        var resources = manager.GetResources();
        for (int index = 0; index < resources.Count; index++)
        {
            var resource = resources[index];
            if (resource.GetCapacity() > 0)
                resourcesPosition[resource] = resource.GetTransform().position;
        }
    }
}
