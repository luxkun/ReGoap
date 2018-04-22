using System;
using System.Collections.Generic;

using ReGoap.Core;
using ReGoap.Unity.FSMExample.OtherScripts;

using UnityEngine;

namespace ReGoap.Unity.FSMExample.Actions
{
    [RequireComponent(typeof(ResourcesBag))]
    public class AddResourceToBankAction : ReGoapAction<string, object>
    {
        private ResourcesBag resourcesBag;
        private Dictionary<string, List<ReGoapState<string, object>>> settingsPerResource;

        protected override void Awake()
        {
            base.Awake();
            resourcesBag = GetComponent<ResourcesBag>();
            settingsPerResource = new Dictionary<string, List<ReGoapState<string, object>>>();
        }

        public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData)
        {
            return base.CheckProceduralCondition(stackData) && stackData.settings.HasKey("bank");
        }

        public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData)
        {
            foreach (var pair in stackData.goalState.GetValues())
            {
                if (pair.Key.StartsWith("collectedResource"))
                {
                    var resourceName = pair.Key.Substring(17);
                    if (settingsPerResource.ContainsKey(resourceName))
                        return settingsPerResource[resourceName];
                    var results = new List<ReGoapState<string, object>>();
                    settings.Set("resourceName", resourceName);
                    // push all available banks
                    foreach (var banksPair in (Dictionary<Bank, Vector3>)stackData.currentState.Get("banks"))
                    {
                        settings.Set("bank", banksPair.Key);
                        settings.Set("bankPosition", banksPair.Value);
                        results.Add(settings.Clone());
                    }
                    settingsPerResource[resourceName] = results;
                    return results;
                }
            }
            return base.GetSettings(stackData);
        }

        public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData)
        {
            if (stackData.settings.HasKey("resourceName"))
                effects.Set("collectedResource" + stackData.settings.Get("resourceName") as string, true);
            return effects;
        }

        public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData)
        {
            if (stackData.settings.HasKey("bank"))
                preconditions.Set("isAtPosition", stackData.settings.Get("bankPosition"));
            if (stackData.settings.HasKey("resourceName"))
                preconditions.Set("hasResource" + stackData.settings.Get("resourceName") as string, true);
            return preconditions;
        }

        public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
        {
            base.Run(previous, next, settings, goalState, done, fail);
            this.settings = settings;
            var bank = settings.Get("bank") as Bank;
            if (bank != null && bank.AddResource(resourcesBag, (string)settings.Get("resourceName")))
            {
                done(this);
            }
            else
            {
                fail(this);
            }
        }
    }
}