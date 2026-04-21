namespace ReGoap.Godot.FSMExample.Tests;

#if GDUNIT4NET_API_V5
using System;
using System.Collections.Generic;

using GdUnit4;

using ReGoap.Core;
using ReGoap.Planner;

using static GdUnit4.Assertions;

[TestSuite]
/// <summary>
/// Covers weighted-random goal selection behavior and deterministic seeding.
/// </summary>
public class WeightedRandomGoalSelectionTests
{
    /// <summary>
    /// Same seed should produce identical pick order across planner instances.
    /// </summary>
    [TestCase]
    public void DeterministicSeedProducesStableGoalOrderAcrossPlanners()
    {
        var firstSequence = PlanSequenceWithSeed(20260421, 40);
        var secondSequence = PlanSequenceWithSeed(20260421, 40);

        AssertThat(string.Join("|", secondSequence)).IsEqual(string.Join("|", firstSequence));
        AssertThat(firstSequence.Contains("Craft Sword")).IsTrue();
        AssertThat(firstSequence.Contains("Random Move Around")).IsTrue();
    }

    /// <summary>
    /// Low-priority goals remain selectable under weighted mode.
    /// </summary>
    [TestCase]
    public void LowPriorityRandomMoveGoalIsStillPickedSometimes()
    {
        var sequence = PlanSequenceWithSeed(1337, 150);
        var randomMoveCount = 0;
        foreach (var goalName in sequence)
        {
            if (goalName == "Random Move Around")
                randomMoveCount++;
        }

        AssertThat(randomMoveCount).IsGreater(0);
        AssertThat(randomMoveCount).IsLess(sequence.Count);
    }

    /// <summary>
    /// When weighted mode is disabled, highest-priority goal must always win.
    /// </summary>
    [TestCase]
    public void WithoutWeightedRandomHighestPriorityGoalAlwaysWins()
    {
        var agent = CreateAgent();
        var settings = new ReGoapPlannerSettings
        {
            UseWeightedRandomGoalSelection = false,
            WeightedRandomUseDeterministicSeed = true,
            WeightedRandomSeed = 99,
        };
        var planner = new ReGoapPlanner<string, object>(settings);

        for (var i = 0; i < 15; i++)
        {
            var goal = planner.Plan(agent);
            AssertThat(goal.GetName()).IsEqual("Craft Sword");
        }
    }

    /// <summary>
    /// Regression guard for known deterministic sequence under a fixed seed.
    /// </summary>
    [TestCase]
    public void DeterministicSeedMatchesKnownFirstPicksSequence()
    {
        var sequence = PlanSequenceWithSeed(424242, 12);
        var actual = string.Join("|", sequence);
        var expected = "Craft Sword|Craft Sword|Craft Sword|Random Move Around|Craft Sword|Craft Sword|Craft Sword|Random Move Around|Craft Sword|Craft Sword|Craft Sword|Craft Sword";
        AssertThat(actual).IsEqual(expected);
    }

    private static List<string> PlanSequenceWithSeed(int seed, int plans)
    {
        var agent = CreateAgent();
        var settings = new ReGoapPlannerSettings
        {
            UseWeightedRandomGoalSelection = true,
            WeightedRandomGoalPriorityPower = 1f,
            WeightedRandomMinimumWeight = 0.05f,
            WeightedRandomUseDeterministicSeed = true,
            WeightedRandomSeed = seed,
        };
        var planner = new ReGoapPlanner<string, object>(settings);

        var sequence = new List<string>(plans);
        for (var i = 0; i < plans; i++)
        {
            var goal = planner.Plan(agent);
            sequence.Add(goal.GetName());
        }

        return sequence;
    }

    private static StubAgent CreateAgent()
    {
        // 10 vs 2.5 priorities map to a 20% random-move baseline in weighted mode.
        return new StubAgent(
            new List<IReGoapGoal<string, object>>
            {
                new StubGoal("Craft Sword", 10f, "behavior", "craft_sword"),
                new StubGoal("Random Move Around", 2.5f, "behavior", "random_move"),
            },
            new List<IReGoapAction<string, object>>
            {
                new StubAction("Craft Sword Action", "behavior", "craft_sword"),
                new StubAction("Random Move Action", "behavior", "random_move"),
            });
    }

    private sealed class StubMemory : IReGoapMemory<string, object>
    {
        private readonly ReGoapState<string, object> worldState = ReGoapState<string, object>.Instantiate();

        public ReGoapState<string, object> GetWorldState() => worldState;
    }

    private sealed class StubAgent : IReGoapAgent<string, object>
    {
        // Minimal agent stub for planner selection tests.
        private readonly List<IReGoapGoal<string, object>> goals;
        private readonly List<IReGoapAction<string, object>> actions;

        public StubAgent(List<IReGoapGoal<string, object>> goals, List<IReGoapAction<string, object>> actions)
        {
            this.goals = goals;
            this.actions = actions;
            Memory = new StubMemory();
        }

        public StubMemory Memory { get; }

        public IReGoapMemory<string, object> GetMemory() => Memory;

        public IReGoapGoal<string, object> GetCurrentGoal() => null;

        public void WarnPossibleGoal(IReGoapGoal<string, object> goal)
        {
        }

        public bool IsActive() => true;

        public List<ReGoapActionState<string, object>> GetStartingPlan() => new();

        public object GetPlanValue(string key) => null;

        public void SetPlanValue(string key, object value)
        {
        }

        public bool HasPlanValue(string target) => false;

        public List<IReGoapGoal<string, object>> GetGoalsSet() => goals;

        public List<IReGoapAction<string, object>> GetActionsSet() => actions;

        public ReGoapState<string, object> InstantiateNewState() => ReGoapState<string, object>.Instantiate();
    }

    private sealed class StubGoal : IReGoapGoal<string, object>
    {
        // Goal stub exposing static priority and a single synthetic effect fact.
        private readonly string name;
        private readonly float priority;
        private readonly ReGoapState<string, object> goalState = ReGoapState<string, object>.Instantiate();
        private Queue<ReGoapActionState<string, object>> plan;

        public StubGoal(string name, float priority, string key, string value)
        {
            this.name = name;
            this.priority = priority;
            goalState.Set(key, value);
        }

        public void Run(Action<IReGoapGoal<string, object>> callback)
        {
        }

        public Queue<ReGoapActionState<string, object>> GetPlan() => plan;

        public string GetName() => name;

        public void Precalculations(IGoapPlanner<string, object> goapPlanner)
        {
        }

        public bool IsGoalPossible() => true;

        public ReGoapState<string, object> GetGoalState() => goalState;

        public float GetPriority() => priority;

        public void SetPlan(Queue<ReGoapActionState<string, object>> path)
        {
            plan = path;
        }

        public float GetErrorDelay() => 0f;
    }

    private sealed class StubAction : IReGoapAction<string, object>
    {
        // Action stub that satisfies planner graph requirements for each goal.
        private readonly string name;
        private readonly ReGoapState<string, object> effects = ReGoapState<string, object>.Instantiate();
        private readonly ReGoapState<string, object> preconditions = ReGoapState<string, object>.Instantiate();
        private readonly ReGoapState<string, object> settings = ReGoapState<string, object>.Instantiate();

        public StubAction(string name, string key, string value)
        {
            this.name = name;
            effects.Set(key, value);
        }

        public List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData) => new() { settings };

        public void Run(IReGoapAction<string, object> previousAction, IReGoapAction<string, object> nextAction, ReGoapState<string, object> actionSettings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
        {
        }

        public void PlanEnter(IReGoapAction<string, object> previousAction, IReGoapAction<string, object> nextAction, ReGoapState<string, object> actionSettings, ReGoapState<string, object> goalState)
        {
        }

        public void PlanExit(IReGoapAction<string, object> previousAction, IReGoapAction<string, object> nextAction, ReGoapState<string, object> actionSettings, ReGoapState<string, object> goalState)
        {
        }

        public void Exit(IReGoapAction<string, object> nextAction)
        {
        }

        public string GetName() => name;

        public bool IsActive() => false;

        public bool IsInterruptable() => true;

        public void AskForInterruption()
        {
        }

        public ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData) => preconditions;

        public ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData) => effects;

        public bool CheckProceduralCondition(GoapActionStackData<string, object> stackData) => true;

        public float GetCost(GoapActionStackData<string, object> stackData) => 1f;

        public void Precalculations(GoapActionStackData<string, object> stackData)
        {
        }

        public string ToString(GoapActionStackData<string, object> stackData) => name;
    }
}
#endif
