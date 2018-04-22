using System;
using System.Collections.Generic;

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
        private List<ReGoapState<string, object>> settingsList;

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
            effects.Set("hasResource" + recipe.GetCraftedResource(), true);

            settingsList = new List<ReGoapState<string, object>>();
        }

        public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData)
        {
            if (settingsList.Count == 0)
                CalculateSettingsList(stackData);
            return settingsList;
        }

        private void CalculateSettingsList(GoapActionStackData<string, object> stackData)
        {
            settingsList.Clear();
            // push all available workstations
            foreach (var workstationsPair in (Dictionary<Workstation, Vector3>)stackData.currentState.Get("workstations"))
            {
                settings.Set("workstation", workstationsPair.Key);
                settings.Set("workstationPosition", workstationsPair.Value);
                settingsList.Add(settings.Clone());
            }
        }

        public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData)
        {
            return base.CheckProceduralCondition(stackData) && stackData.settings.HasKey("workstation");
        }

        public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData)
        {
            if (stackData.settings.HasKey("workstationPosition"))
                preconditions.Set("isAtPosition", stackData.settings.Get("workstationPosition"));
            return preconditions;
        }

        public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
        {
            base.Run(previous, next, settings, goalState, done, fail);
            var workstation = settings.Get("workstation") as Workstation;
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
    }
}
