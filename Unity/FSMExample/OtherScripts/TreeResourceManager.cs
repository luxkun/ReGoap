using UnityEngine;
using System.Collections;

public class TreeResourceManager : ResourceManager
{
    public static TreeResourceManager instance;

    protected override void Awake()
    {
        base.Awake();
        if (instance != null)
            throw new UnityException("[TreeResourceManager] Can have only one instance per scene.");
        instance = this;
    }
}
