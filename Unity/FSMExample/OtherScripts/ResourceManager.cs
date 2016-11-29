using UnityEngine;
using System.Collections;

// one resourcemanager per type
public class ResourceManager : MonoBehaviour, IResourceManager
{
    public string resourceName = "ResourceName";
    public MonoBehaviour[] rawResources; // add resources directly in Unity
    public IResource[] resources;
    private int currentIndex;

    #region UnityFunctions
    protected virtual void Awake()
    {
        currentIndex = 0;

        resources = new IResource[rawResources.Length];
        for (int index = 0; index < rawResources.Length; index++)
        {
            var resource = rawResources[index];
            var iresource = resource as IResource;
            if (iresource != null)
                resources[index] = iresource;
            else
                throw new UnityException(string.Format("[{0}] rawResources has a behaviour which does not implement IResource.", GetType().FullName));
        }
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
        return resourceName;
    }

    public virtual int GetResourcesCount()
    {
        return resources.Length;
    }

    public virtual IResource[] GetResources()
    {
        return resources;
    }

    public virtual IResource GetResource()
    {
        var result = resources[currentIndex];
        currentIndex = currentIndex++%resources.Length;
        return result;
    }
    #endregion
}

public interface IResourceManager
{
    string GetResourceName();
    int GetResourcesCount();
    IResource[] GetResources();
    // preferably should get a different transform every call
    IResource GetResource();
}
