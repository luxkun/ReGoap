using UnityEngine;
using System.Collections;

// one resourcemanager per type
public class ResourceManager : MonoBehaviour, IResourceManager
{
    public string ResourceName = "ResourceName";
    public MonoBehaviour[] RawResources; // add resources directly in Unity
    public IResource[] Resources;
    private int currentIndex;

    #region UnityFunctions
    protected virtual void Awake()
    {
        currentIndex = 0;

        Resources = new IResource[RawResources.Length];
        for (int index = 0; index < RawResources.Length; index++)
        {
            var resource = RawResources[index];
            var iresource = resource as IResource;
            if (iresource != null)
                Resources[index] = iresource;
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
        return ResourceName;
    }

    public virtual int GetResourcesCount()
    {
        return Resources.Length;
    }

    public virtual IResource[] GetResources()
    {
        return Resources;
    }

    public virtual IResource GetResource()
    {
        var result = Resources[currentIndex];
        currentIndex = currentIndex++%Resources.Length;
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
