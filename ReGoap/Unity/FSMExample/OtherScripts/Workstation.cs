using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    public class Workstation : MonoBehaviour
    {
        public bool CraftResource(ResourcesBag crafterBag, IRecipe recipe, float value = 1f)
        {
            // check Recipe, could be removed since the agent already check for recipe items
            foreach (var pair in recipe.GetNeededResources())
            {
                if (crafterBag.GetResource(pair.Key) < pair.Value * value)
                {
                    //throw new UnityException(string.Format("[Workstation] Trying to craft recipe '{0}' without having enough '{1}' resources.", recipe.GetCraftedResource(), pair.Key));
                    return false;
                }
            }
            // if can go loop again and remove needed resources
            foreach (var pair in recipe.GetNeededResources())
            {
                crafterBag.RemoveResource(pair.Key, pair.Value * value);
            }
            var resource = recipe.GetCraftedResource();
            crafterBag.AddResource(resource, value);
            return true;
        }
    }
}