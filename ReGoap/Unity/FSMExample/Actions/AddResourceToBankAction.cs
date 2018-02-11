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

        protected override void Awake()
        {
            base.Awake();
            resourcesBag = GetComponent<ResourcesBag>();
        }

        public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData)
        {
            settings.Clear();
            foreach (var pair in stackData.goalState.GetValues())
            {
                if (pair.Key.StartsWith("collectedResource"))
                {
                    var resourceName = pair.Key.Substring(17);
                    settings.Set("ResourceName", resourceName);
                    break;
                }
            }
            return base.GetSettings(stackData);
        }

        public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData)
        {
            effects.Clear();

            foreach (var pair in stackData.goalState.GetValues())
            {
                if (pair.Key.StartsWith("collectedResource"))
                {
                    var resourceName = pair.Key.Substring(17);
                    effects.Set("collectedResource" + resourceName, true);
                    break;
                }
            }

            return effects;
        }

        public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData)
        {
            var bankPosition = agent.GetMemory().GetWorldState().Get("nearestBankPosition") as Vector3?;

            preconditions.Clear();
            preconditions.Set("isAtPosition", bankPosition);

            foreach (var pair in stackData.goalState.GetValues())
            {
                if (pair.Key.StartsWith("collectedResource"))
                {
                    var resourceName = pair.Key.Substring(17);
                    preconditions.Set("hasResource" + resourceName, true);
                    break;
                }
            }

            return preconditions;
        }


        public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
        {
            base.Run(previous, next, settings, goalState, done, fail);
            this.settings = settings;
            var bank = agent.GetMemory().GetWorldState().Get("nearestBank") as Bank;
            if (bank != null && bank.AddResource(resourcesBag, (string)settings.Get("ResourceName")))
            {
                done(this);
            }
            else
            {
                fail(this);
            }
        }

        public override string ToString()
        {
            return string.Format("GoapAction('{0}')", Name);
        }
    }
}