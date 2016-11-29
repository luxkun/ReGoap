using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "New Recipe", order = 1)]
public class Recipe : ScriptableObject, IRecipe
{
    public List<string> neededResourcesName;
    public string craftName;

    public Dictionary<string, float> GetNeededResources()
    {
        var dict = new Dictionary<string, float>();
        foreach (var resourceName in neededResourcesName)
        {
            // could implement a more flexible system that has dynamic resources's count (need to create ad-hoc actions or a generic one that handle number of resources)
            dict[resourceName] = 1f;
        }
        return dict;
    }

    public string GetCraftedResource()
    {
        return craftName;
    }
}

public interface IRecipe
{
    Dictionary<string, float> GetNeededResources();
    string GetCraftedResource();
}
