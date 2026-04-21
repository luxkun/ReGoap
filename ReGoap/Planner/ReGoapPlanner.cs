using System;
using System.Collections.Generic;
using System.Linq;
using ReGoap.Core;
using ReGoap.Utilities;

namespace ReGoap.Planner
{
    /// <summary>
    /// Main GOAP planner implementation.
    /// Selects a goal and computes an action queue via A* over world-state diffs.
    /// </summary>
    public class ReGoapPlanner<T, W> : IGoapPlanner<T, W>
    {
        private readonly Random random;
        private IReGoapAgent<T, W> goapAgent;
        private IReGoapGoal<T, W> currentGoal;
        public bool Calculated;
        private readonly AStar<ReGoapState<T, W>> astar;
        private readonly ReGoapPlannerSettings settings;

        /// <summary>
        /// Creates a planner with optional custom settings.
        /// </summary>
        public ReGoapPlanner(ReGoapPlannerSettings settings = null)
        {
            this.settings = settings ?? new ReGoapPlannerSettings();
            astar = new AStar<ReGoapState<T, W>>(this.settings.MaxNodesToExpand);
            random = this.settings.WeightedRandomUseDeterministicSeed
                ? new Random(this.settings.WeightedRandomSeed)
                : new Random();
        }

        /// <summary>
        /// Computes a plan for <paramref name="agent"/> and returns the chosen goal.
        /// </summary>
        public IReGoapGoal<T, W> Plan(IReGoapAgent<T, W> agent, IReGoapGoal<T, W> blacklistGoal = null, Queue<ReGoapActionState<T, W>> currentPlan = null, Action<IReGoapGoal<T, W>> callback = null)
        {
            if (ReGoapLogger.Level == ReGoapLogger.DebugLevel.Full)
                ReGoapLogger.Log("[ReGoalPlanner] Starting planning calculation for agent: " + agent);
            goapAgent = agent;
            Calculated = false;
            currentGoal = null;
            var possibleGoals = new List<IReGoapGoal<T, W>>();
            foreach (var goal in goapAgent.GetGoalsSet())
            {
                if (goal == blacklistGoal)
                    continue;
                goal.Precalculations(this);
                if (goal.IsGoalPossible())
                    possibleGoals.Add(goal);
            }
            possibleGoals.Sort((x, y) => x.GetPriority().CompareTo(y.GetPriority()));

            var currentState = agent.GetMemory().GetWorldState();

            while (possibleGoals.Count > 0)
            {
                currentGoal = SelectNextGoal(possibleGoals);
                possibleGoals.Remove(currentGoal);
                var goalState = currentGoal.GetGoalState();

                // Optional fast pre-check for non-dynamic action sets.
                // can't work with dynamic actions, of course
                if (!settings.UsingDynamicActions)
                {
                    var wantedGoalCheck = currentGoal.GetGoalState();
                    GoapActionStackData<T, W> stackData;
                    stackData.agent = goapAgent;
                    stackData.currentState = currentState;
                    stackData.goalState = goalState;
                    stackData.next = null;
                    stackData.settings = null;
                    // we check if the goal can be archived through actions first, so we don't brute force it with A* if we can't
                    foreach (var action in goapAgent.GetActionsSet())
                    {
                        action.Precalculations(stackData);
                        if (!action.CheckProceduralCondition(stackData))
                        {
                            continue;
                        }
                        // check if the effects of all actions can archieve currentGoal
                        var previous = wantedGoalCheck;
                        wantedGoalCheck = ReGoapState<T, W>.Instantiate();
                        previous.MissingDifference(action.GetEffects(stackData), ref wantedGoalCheck);
                    }
                    // finally push the current world state
                    var current = wantedGoalCheck;
                    wantedGoalCheck = ReGoapState<T, W>.Instantiate();
                    current.MissingDifference(GetCurrentAgent().GetMemory().GetWorldState(), ref wantedGoalCheck);
                    // can't validate goal 
                    if (wantedGoalCheck.Count > 0)
                    {
                        currentGoal = null;
                        continue;
                    }
                }

                goalState = goalState.Clone();
                var leaf = (ReGoapNode<T, W>)astar.Run(
                    ReGoapNode<T, W>.Instantiate(this, goalState, null, null, null), goalState, settings.MaxIterations, settings.PlanningEarlyExit, debugPlan : settings.DebugPlan);
                if (leaf == null)
                {
                    currentGoal = null;
                    continue;
                }

                var result = leaf.CalculatePath();
                if (currentPlan != null && currentPlan == result)
                {
                    currentGoal = null;
                    break;
                }
                if (result.Count == 0)
                {
                    currentGoal = null;
                    continue;
                }
                currentGoal.SetPlan(result);
                break;
            }
            Calculated = true;

            if (callback != null)
                callback(currentGoal);
            if (currentGoal != null)
            {
                ReGoapLogger.Log(string.Format("[ReGoapPlanner] Calculated plan for goal '{0}', plan length: {1}", currentGoal, currentGoal.GetPlan().Count));
                if (ReGoapLogger.Level == ReGoapLogger.DebugLevel.Full)
                {
                    int i = 0;
                    GoapActionStackData<T, W> stackData;
                    stackData.agent = agent;
                    stackData.currentState = currentState;
                    stackData.goalState = currentGoal.GetGoalState();
                    stackData.next = null;
                    foreach (var action in currentGoal.GetPlan())
                    {
                        stackData.settings = action.Settings;
                        ReGoapLogger.Log(string.Format("[ReGoapPlanner] {0}) {1}", i++, action.Action.ToString(stackData)));
                    }
                }
            }
            else
                ReGoapLogger.LogWarning("[ReGoapPlanner] Error while calculating plan.");
            return currentGoal;
        }

        private IReGoapGoal<T, W> SelectNextGoal(List<IReGoapGoal<T, W>> possibleGoals)
        {
            if (!settings.UseWeightedRandomGoalSelection || possibleGoals.Count == 1)
                return possibleGoals[possibleGoals.Count - 1];

            var power = Math.Max(0.01f, settings.WeightedRandomGoalPriorityPower);
            var minWeight = Math.Max(0.000001f, settings.WeightedRandomMinimumWeight);

            var weights = new float[possibleGoals.Count];
            float total = 0f;
            for (var i = 0; i < possibleGoals.Count; i++)
            {
                var priority = possibleGoals[i].GetPriority();
                var clampedPriority = Math.Max(0f, priority);
                var weight = (float)Math.Pow(clampedPriority, power);
                if (weight < minWeight)
                    weight = minWeight;
                weights[i] = weight;
                total += weight;
            }

            if (total <= 0f)
                return possibleGoals[possibleGoals.Count - 1];

            var roll = (float)(random.NextDouble() * total);
            float cumulative = 0f;
            for (var i = 0; i < possibleGoals.Count; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative)
                    return possibleGoals[i];
            }

            return possibleGoals.Last();
        }

        public IReGoapGoal<T, W> GetCurrentGoal()
        {
            return currentGoal;
        }

        /// <summary>
        /// Returns agent currently being planned.
        /// </summary>
        public IReGoapAgent<T, W> GetCurrentAgent()
        {
            return goapAgent;
        }

        /// <summary>
        /// Returns true while a planning call is in progress.
        /// </summary>
        public bool IsPlanning()
        {
            return !Calculated;
        }

        /// <summary>
        /// Returns active planner settings.
        /// </summary>
        public ReGoapPlannerSettings GetSettings()
        {
            return settings;
        }
    }
}
