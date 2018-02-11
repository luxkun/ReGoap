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
            var newNeededResourceName = GetNeededResourceFromGoal(stackData.goalState);
            preconditions.Clear();
            if (newNeededResourceName != null)
            {
                var wantedResource = agent.GetMemory().GetWorldState().Get("nearest" + newNeededResourceName) as IResource;
                if (wantedResource != null)
                {
                    preconditions.Set("isAtPosition", agent.GetMemory().GetWorldState()
                        .Get(string.Format("nearest{0}Position", newNeededResourceName)) as Vector3?);
                }
            }
            return preconditions;
        }

        public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData)
        {
            var newNeededResourceName = GetNeededResourceFromGoal(stackData.goalState);
            effects.Clear();
            if (newNeededResourceName != null)
            {
                var wantedResource = agent.GetMemory().GetWorldState().Get("nearest" + newNeededResourceName) as IResource;
                if (wantedResource != null)
                {
                    effects.Set("hasResource" + newNeededResourceName, true);
                }
            }
            return effects;
        }

        public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData)
        {
            var newNeededResourceName = GetNeededResourceFromGoal(stackData.goalState);
            settings.Clear();
            if (newNeededResourceName != null)
            {
                var wantedResource = agent.GetMemory().GetWorldState().Get("nearest" + newNeededResourceName) as IResource;
                if (wantedResource != null)
                {
                    settings.Set("ResourcePosition", (Vector3)agent.GetMemory().GetWorldState()
                        .Get(string.Format("nearest{0}Position", newNeededResourceName)));
                    settings.Set("Resource", wantedResource);
                    return base.GetSettings(stackData);
                }
            }
            return new List<ReGoapState<string, object>>();
        }

        public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData)
        {
            return base.CheckProceduralCondition(stackData) && bag != null;
        }

        public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
        {
            base.Run(previous, next, settings, goalState, done, fail);

            var thisSettings = settings;
            resourcePosition = (Vector3)thisSettings.Get("ResourcePosition");
            resource = (IResource)thisSettings.Get("Resource");

            if (resource == null || resource.GetCapacity() < ResourcePerAction)
                failCallback(this);
            else
            {
                gatherCooldown = Time.time + TimeToGather;
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
            }
        }
    }
}