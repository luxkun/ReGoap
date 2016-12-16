using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ResourcesBag))]
public class CraftRecipeAction : GoapAction
{
    public ScriptableObject rawRecipe;
    private IRecipe recipe;
    private ResourcesBag resourcesBag;

    protected override void Awake()
    {
        base.Awake();
        recipe = rawRecipe as IRecipe;
        if (recipe == null)
            throw new UnityException("[CraftRecipeAction] The rawRecipe ScriptableObject must implement IRecipe.");
        resourcesBag = GetComponent<ResourcesBag>();

        // could implement a more flexible system that handles dynamic resources's count
        foreach (var pair in recipe.GetNeededResources())
        {
            preconditions.Set("has" + pair.Key, true);
        }
        preconditions.Set<Transform>("isAtTransform", null);
        preconditions.Set("has" + recipe.GetCraftedResource(), false); // do not permit duplicates in the bag
        effects.Set("has" + recipe.GetCraftedResource(), true);
    }

    public override void Precalculations(IReGoapAgent goapAgent, ReGoapState goalState)
    {
        base.Precalculations(goapAgent, goalState);
        var workstation = GetNearestWorkstation();
        if (workstation != null)
            preconditions.Set("isAtTransform", workstation.transform);
    }

    public override void Run(IReGoapAction previous, IReGoapAction next, ReGoapState goalState, Action<IReGoapAction> done, Action<IReGoapAction> fail)
    {
        base.Run(previous, next, goalState, done, fail);
        var workstation = GetNearestWorkstation();
        if (workstation.CraftResource(resourcesBag, recipe))
        {
            done(this);
        }
        else
        {
            fail(this);
        }
    }

    private Workstation GetNearestWorkstation()
    {
        return agent.GetMemory().GetWorldState().Get<Workstation>("nearestWorkstation");
    }

    public override string ToString()
    {
        return string.Format("GoapAction('{0}', '{1}')", Name, recipe.GetCraftedResource());
    }
}
