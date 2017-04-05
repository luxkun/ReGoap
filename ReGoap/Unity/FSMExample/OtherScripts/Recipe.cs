using System.Collections.Generic;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    [CreateAssetMenu(fileName = "Recipe", menuName = "New Recipe", order = 1)]
    public class Recipe : ScriptableObject, IRecipe
    {
        public List<string> NeededResourcesName;
        public string CraftName;

        public Dictionary<string, float> GetNeededResources()
        {
            var dict = new Dictionary<string, float>();
            foreach (var resourceName in NeededResourcesName)
            {
                // could implement a more flexible system that has dynamic resources's count (need to create ad-hoc actions or a generic one that handle number of resources)
                dict[resourceName] = 1f;
            }
            return dict;
        }

        public string GetCraftedResource()
        {
            return CraftName;
        }
    }

    public interface IRecipe
    {
        Dictionary<string, float> GetNeededResources();
        string GetCraftedResource();
    }
}