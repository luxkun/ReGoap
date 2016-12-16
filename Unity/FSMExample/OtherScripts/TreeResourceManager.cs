using UnityEngine;
using System.Collections;

public class TreeResourceManager : ResourceManager
{
    public static TreeResourceManager Instance;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != null)
            throw new UnityException("[TreeResourceManager] Can have only one instance per scene.");
        Instance = this;
    }
}
