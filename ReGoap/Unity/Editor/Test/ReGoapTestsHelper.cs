using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Unity.Test;
using NUnit.Framework;
using UnityEngine;

namespace ReGoap.Unity.Editor.Test
{
    public class ReGoapTestsHelper
    {
        public static ReGoapTestAction GetCustomAction(GameObject gameObject, string name, Dictionary<string, object> preconditionsBools,
            Dictionary<string, object> effectsBools, int cost = 1)
        {
            var effects = ReGoapState<string, object>.Instantiate();
            var preconditions = ReGoapState<string, object>.Instantiate();
            var customAction = gameObject.AddComponent<ReGoapTestAction>();
            customAction.Name = name;
            customAction.Init();
            foreach (var pair in effectsBools)
                effects.Set(pair.Key, pair.Value);
            customAction.SetEffects(effects);
            foreach (var pair in preconditionsBools)
                preconditions.Set(pair.Key, pair.Value);
            customAction.SetPreconditions(preconditions);
            customAction.Cost = cost;
            return customAction;
        }

        public static ReGoapTestGoal GetCustomGoal(GameObject gameObject, string name, Dictionary<string, object> goalState, int priority = 1)
        {
            var customGoal = gameObject.AddComponent<ReGoapTestGoal>();
            customGoal.Name = name;
            customGoal.SetPriority(priority);
            customGoal.Init();
            var goal = ReGoapState<string, object>.Instantiate();
            foreach (var pair in goalState)
            {
                goal.Set(pair.Key, pair.Value);
            }
            customGoal.SetGoalState(goal);
            return customGoal;
        }

        public static void ApplyAndValidatePlan(IReGoapGoal<string, object> plan, ReGoapTestAgent agent,  ReGoapTestMemory memory)
        {
            GoapActionStackData<string, object> stackData;
            stackData.agent = agent;
            stackData.currentState = memory.GetWorldState();
            stackData.goalState = plan.GetGoalState();
            stackData.next = null;
            stackData.settings = null;
            foreach (var action in plan.GetPlan())
            {
                stackData.settings = action.Settings;
                Assert.That(action.Action.GetPreconditions(stackData).MissingDifference(stackData.currentState, 1) == 0);
                foreach (var effectsPair in action.Action.GetEffects(stackData).GetValues())
                {   // in a real game this should be done by memory itself
                    //  e.x. isNearTarget = (transform.position - target.position).magnitude < minRangeForCC
                    memory.SetValue(effectsPair.Key, effectsPair.Value);
                }
            }
            Assert.That(plan.GetGoalState().MissingDifference(memory.GetWorldState(), 1) == 0);
        }
    }
}
