using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// craftable items as well primitive resources
public class Resource : MonoBehaviour, IResource
{
    public string ResourceName;
    public float Capacity = 1f;
    protected float startingCapacity;

    protected virtual void Awake()
    {
        startingCapacity = Capacity;
    }

    public string GetName()
    {
        return ResourceName;
    }

    public virtual Transform GetTransform()
    {
        return transform;
    }

    public virtual float GetCapacity()
    {
        return Capacity;
    }

    public virtual void RemoveResource(float value)
    {
        Capacity -= value;
    }
}

public interface IResource
{
    string GetName();
    Transform GetTransform();
    float GetCapacity();
    void RemoveResource(float value);
}

