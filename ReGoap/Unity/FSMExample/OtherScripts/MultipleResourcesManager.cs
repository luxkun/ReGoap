using System.Collections.Generic;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    public class MultipleResourcesManager : MonoBehaviour
    {
        public static MultipleResourcesManager Instance;

        public Dictionary<string, IResourceManager> Resources;

        void Awake()
        {
            if (Instance != null)
                throw new UnityException("[ResourcesManager] Can have only one instance per scene.");
            Instance = this;
            var childResources = GetComponentsInChildren<IResource>();
            Resources = new Dictionary<string, IResourceManager>(childResources.Length);
            foreach (var resource in childResources)
            {
                if (!Resources.ContainsKey(resource.GetName()))
                {
                    var manager = gameObject.AddComponent<ResourceManager>();
                    manager.ResourceName = resource.GetName();
                    Resources[resource.GetName()] = manager;
                }
                Resources[resource.GetName()].AddResource(resource);
            }
        }
    }
}
