using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// craftable items as well primitive resources
public class Resource : MonoBehaviour, IResource
{
    public string resourceName;
    public float capacity = 1f;
    protected float startingCapacity;

    protected virtual void Awake()
    {
        startingCapacity = capacity;
    }

    public string GetName()
    {
        return resourceName;
    }

    public virtual Transform GetTransform()
    {
        return transform;
    }

    public virtual float GetCapacity()
    {
        return capacity;
    }

    public virtual void RemoveResource(float value)
    {
        capacity -= value;
    }
}

public interface IResource
{
    string GetName();
    Transform GetTransform();
    float GetCapacity();
    void RemoveResource(float value);
}

