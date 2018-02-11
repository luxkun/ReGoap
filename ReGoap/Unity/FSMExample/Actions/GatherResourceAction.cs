using System;
using System.Collections.Generic;

using ReGoap.Core;
using ReGoap.Unity.FSMExample.OtherScripts;
using ReGoap.Utilities;

using UnityEngine;

namespace ReGoap.Unity.FSMExample.Actions
{
    public class GatherResourceAction : ReGoapAction<string, object>
    {
        public float MaxResourcesCount = 5.0f;
        public float ResourcesCostMultiplier = 10.0f;
        public float ReservedCostMultiplier = 50.0f;

        public bool ExpandOnAllResources = false;

        public float TimeToGather = 0.5f;
        public float ResourcePerAction = 1f;
        protected ResourcesBag bag;
        protected Vector3? resourcePosition;
        protected IResource resource;

        private float gatherCooldown;

        protected override void Awake()
        {
            base.Awake();

            bag = GetComponent<ResourcesBag>();
        }

        protected virtual string GetNeededResourceFromGoal(ReGoapState<string, object> goalState)
        {
            foreach (var pair in goalState.GetValues())
            {
                if (pair.Key.StartsWith("hasResource"))
                {
                    return pair.Key.Substring(11);
                }
            }
            return null;
        }

        public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData)
        {
            preconditions.Clear();
            if (stackData.settings.HasKey("resource"))
            {
                preconditions.Set("isAtPosition", stackData.settings.Get("resourcePosition"));
            }
            return preconditions;
        }

        public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData)
        {
            effects.Clear();
            if (stackData.settings.HasKey("resource"))
            {
                effects.Set("hasResource" + ((IResource)stackData.settings.Get("resource")).GetName(), true);
            }
            return effects;
        }

        public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData)
        {
            var newNeededResourceName = GetNeededResourceFromGoal(stackData.goalState);
            settings.Clear();
            if (newNeededResourceName != null && stackData.currentState.HasKey("resource" + newNeededResourceName))
            {
                var results = new List<ReGoapState<string, object>>();
                Sensors.ResourcePair best = new Sensors.ResourcePair();
                var bestScore = float.MaxValue;
                foreach (var wantedResource in (List<Sensors.ResourcePair>)stackData.currentState.Get("resource" + newNeededResourceName))
                {
                    if (wantedResource.resource.GetCapacity() < ResourcePerAction) continue;
                    // expanding on all resources is VERY expansive, expanding on the closest one is usually the best decision
                    if (ExpandOnAllResources)
                    {
                        settings.Set("resourcePosition", wantedResource.position);
                        settings.Set("resource", wantedResource.resource);
                        results.Add(settings.Clone());
                    }
                    else
                    {
                        var score = stackData.currentState.HasKey("isAtPosition") ? (wantedResource.position - (Vector3)stackData.currentState.Get("isAtPosition")).magnitude : 0.0f;
                        score += ReservedCostMultiplier * wantedResource.resource.GetReserveCount();
                        score += ResourcesCostMultiplier * (MaxResourcesCount - wantedResource.resource.GetCapacity());
                        if (score < bestScore)
                        {
                            bestScore = score;
                            best = wantedResource;
                        }
                    }
                }
                if (!ExpandOnAllResources)
                {
                    settings.Set("resourcePosition", best.position);
                    settings.Set("resource", best.resource);
                    results.Add(settings.Clone());
                }
                return results;
            }
            return new List<ReGoapState<string, object>>();
        }

        public override float GetCost(GoapActionStackData<string, object> stackData)
        {
            var extraCost = 0.0f;
            if (stackData.settings.HasKey("resource"))
            {
                var resource = (Resource)stackData.settings.Get("resource");
                extraCost += ReservedCostMultiplier * resource.GetReserveCount();
                extraCost += ResourcesCostMultiplier * (MaxResourcesCount - resource.GetCapacity());
            }
            return base.GetCost(stackData) + extraCost;
        }

        public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData)
        {
            return base.CheckProceduralCondition(stackData) && bag != null && stackData.settings.HasKey("resource");
        }

        public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
        {
            base.Run(previous, next, settings, goalState, done, fail);

            var thisSettings = settings;
            resourcePosition = (Vector3)thisSettings.Get("resourcePosition");
            resource = (IResource)thisSettings.Get("resource");

            if (resource == null || resource.GetCapacity() < ResourcePerAction)
                failCallback(this);
            else
            {
                gatherCooldown = Time.time + TimeToGather;
            }
        }

        public override void PlanEnter(IReGoapAction<string, object> previousAction, IReGoapAction<string, object> nextAction, ReGoapState<string, object> settings, ReGoapState<string, object> goalState)
        {
            if (settings.HasKey("resource"))
            {
                ((IResource)settings.Get("resource")).Reserve(GetHashCode());
            }
        }
        public override void PlanExit(IReGoapAction<string, object> previousAction, IReGoapAction<string, object> nextAction, ReGoapState<string, object> settings, ReGoapState<string, object> goalState)
        {
            if (settings.HasKey("resource"))
            {
                ((IResource)settings.Get("resource")).Unreserve(GetHashCode());
            }
        }

        protected void Update()
        {
            if (resource == null || resource.GetCapacity() < ResourcePerAction)
            {
                failCallback(this);
                return;
            }
            if (Time.time > gatherCooldown)
            {
                gatherCooldown = float.MaxValue;
                ReGoapLogger.Log("[GatherResourceAction] acquired " + ResourcePerAction + " " + resource.GetName());
                resource.RemoveResource(ResourcePerAction);
                bag.AddResource(resource.GetName(), ResourcePerAction);
                doneCallback(this);
                if (settings.HasKey("resource"))
                {
                    ((IResource)settings.Get("resource")).Unreserve(GetHashCode());
                }
            }
        }
    }
}