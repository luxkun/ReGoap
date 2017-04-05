using System.Collections.Generic;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{ // one resourcemanager per type
    public class ResourceManager : MonoBehaviour, IResourceManager
    {
        private List<IResource> resources;
        private int currentIndex;
        public string ResourceName;

        #region UnityFunctions
        protected virtual void Awake()
        {
            currentIndex = 0;
            resources = new List<IResource>();
        }

        protected virtual void Start()
        {
        }

        protected virtual void Update()
        {
        }

        protected virtual void FixedUpdate()
        {
        }
        #endregion

        #region IResourceManager
        public virtual string GetResourceName()
        {
            return ResourceName;
        }

        public virtual int GetResourcesCount()
        {
            return resources.Count;
        }

        public virtual List<IResource> GetResources()
        {
            return resources;
        }

        public virtual IResource GetResource()
        {
            var result = resources[currentIndex];
            currentIndex = currentIndex++ % resources.Count;
            return result;
        }

        public void AddResource(IResource resource)
        {
            resources.Add(resource);
        }

        #endregion
    }

    public interface IResourceManager
    {
        string GetResourceName();
        int GetResourcesCount();
        List<IResource> GetResources();
        // preferably should get a different transform every call
        IResource GetResource();
        void AddResource(IResource resource);
    }
}