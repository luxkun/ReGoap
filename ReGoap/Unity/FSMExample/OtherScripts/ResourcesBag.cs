using System.Collections.Generic;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    public class ResourcesBag : MonoBehaviour {
        private Dictionary<string, float> resources;

        void Awake()
        {
            resources = new Dictionary<string, float>();
        }

        public void AddResource(string resourceName, float value)
        {
            if (!resources.ContainsKey(resourceName))
                resources[resourceName] = 0;
            resources[resourceName] += value;
        }

        public float GetResource(string resourceName)
        {
            var value = 0f;
            resources.TryGetValue(resourceName, out value);
            return value;
        }

        public Dictionary<string, float> GetResources()
        {
            return resources;
        }

        public void RemoveResource(string resourceName, float value)
        {
            resources[resourceName] -= value;
        }
    }
}
