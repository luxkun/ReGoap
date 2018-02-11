using System;
using ReGoap.Core;
using ReGoap.Unity.FSMExample.OtherScripts;
using ReGoap.Utilities;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.Actions
{
    [RequireComponent(typeof(ResourcesBag))]
    public class CraftRecipeAction : ReGoapAction<string, object>
    {
        public ScriptableObject RawRecipe;
        private IRecipe recipe;
        private ResourcesBag resourcesBag;

        protected override void Awake()
        {
            base.Awake();
            recipe = RawRecipe as IRecipe;
            if (recipe == null)
                throw new UnityException("[CraftRecipeAction] The rawRecipe ScriptableObject must implement IRecipe.");
            resourcesBag = GetComponent<ResourcesBag>();

            // could implement a more flexible system that handles dynamic resources's count
            foreach (var pair in recipe.GetNeededResources())
            {
                preconditions.Set("hasResource" + pair.Key, true);
            }
            // false preconditions are not supported
            //preconditions.Set("hasResource" + recipe.GetCraftedResource(), false); // do not permit duplicates in the bag
            effects.Set("hasResource" + recipe.GetCraftedResource(), true);
        }

        public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData)
        {
            preconditions.Set("isAtPosition", agent.GetMemory().GetWorldState().Get("nearestWorkstationPosition") as Vector3?);
            return preconditions;
        }

        public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
        {
            base.Run(previous, next, settings, goalState, done, fail);
            var workstation = agent.GetMemory().GetWorldState().Get("nearestWorkstation") as Workstation;
            if (workstation != null && workstation.CraftResource(resourcesBag, recipe))
            {
                ReGoapLogger.Log("[CraftRecipeAction] crafted recipe " + recipe.GetCraftedResource());
                done(this);
            }
            else
            {
                fail(this);
            }
        }

        public override string ToString()
        {
            return string.Format("GoapAction('{0}', '{1}')", Name, recipe.GetCraftedResource());
        }
    }
}
