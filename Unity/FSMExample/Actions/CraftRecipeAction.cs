using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ResourcesBag))]
public class CraftRecipeAction : GoapAction
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

    public override void Precalculations(IReGoapAgent goapAgent, ReGoapState goalState)
    {
        base.Precalculations(goapAgent, goalState);
        var workstationPosition = agent.GetMemory().GetWorldState().Get<Vector3>("nearestWorkstationPosition");
        preconditions.Set("isAtPosition", workstationPosition);
        //effects.Set("isAtPosition", Vector3.zero);
    }

    public override void Run(IReGoapAction previous, IReGoapAction next, IReGoapActionSettings settings, ReGoapState goalState, Action<IReGoapAction> done, Action<IReGoapAction> fail)
    {
        base.Run(previous, next, settings, goalState, done, fail);
        var workstation = agent.GetMemory().GetWorldState().Get<Workstation>("nearestWorkstation");
        if (workstation.CraftResource(resourcesBag, recipe))
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
