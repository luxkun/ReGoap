namespace ReGoap.Godot.FSMExample.Tests;

#if GDUNIT4NET_API_V5
using GdUnit4;

using ReGoap.Core;
using ReGoap.Godot.FSMExample.World;

using static GdUnit4.Assertions;

[TestSuite]
/// <summary>
/// Covers dynamic random-move goal threshold behavior.
/// </summary>
public class RandomMoveGoalTests
{
    /// <summary>
    /// Goal target should require at least the next random move count.
    /// </summary>
    [TestCase]
    [RequireGodotRuntime]
    public void PrecalculationsTargetsNextMoveCountWithGreaterOrEqualCondition()
    {
        var goal = new RandomMoveGoal();
        goal._Ready();

        var planner = new StubPlanner(3);
        goal.Precalculations(planner);

        var firstCondition = goal.GetGoalState().Get("randomMoveCount") as ReGoapCondition;
        AssertThat(firstCondition).IsNotNull();
        AssertBool(firstCondition.IsSatisfiedBy(4)).IsTrue();
        AssertBool(firstCondition.IsSatisfiedBy(5)).IsTrue();
        AssertBool(firstCondition.IsSatisfiedBy(3)).IsFalse();

        AssertThat(firstCondition.Operator).IsEqual(ReGoapConditionOperator.GreaterOrEqual);
        AssertThat(firstCondition.Value).IsEqual(4);

        planner.SetMoveCount(4);
        goal.Precalculations(planner);

        var secondCondition = goal.GetGoalState().Get("randomMoveCount") as ReGoapCondition;
        AssertThat(secondCondition).IsNotNull();
        AssertThat(secondCondition.Operator).IsEqual(ReGoapConditionOperator.GreaterOrEqual);
        AssertThat(secondCondition.Value).IsEqual(5);
        AssertBool(secondCondition.IsSatisfiedBy(6)).IsTrue();
        AssertBool(secondCondition.IsSatisfiedBy(4)).IsFalse();
    }

    private sealed class StubPlanner : ReGoap.Planner.IGoapPlanner<string, object>
    {
        // Minimal planner stub: only agent access is needed by goal precalculations.
        private readonly StubAgent agent;

        public StubPlanner(int moveCount)
        {
            agent = new StubAgent(moveCount);
        }

        public void SetMoveCount(int moveCount)
        {
            agent.Memory.GetWorldState().Set("randomMoveCount", moveCount);
        }

        public ReGoap.Core.IReGoapGoal<string, object> Plan(ReGoap.Core.IReGoapAgent<string, object> goapAgent, ReGoap.Core.IReGoapGoal<string, object> blacklistGoal, System.Collections.Generic.Queue<ReGoapActionState<string, object>> currentPlan, System.Action<ReGoap.Core.IReGoapGoal<string, object>> callback)
        {
            throw new System.NotImplementedException();
        }

        public ReGoap.Core.IReGoapGoal<string, object> GetCurrentGoal() => null;

        public ReGoap.Core.IReGoapAgent<string, object> GetCurrentAgent() => agent;

        public bool IsPlanning() => false;

        public ReGoap.Planner.ReGoapPlannerSettings GetSettings() => new();
    }

    private sealed class StubAgent : ReGoap.Core.IReGoapAgent<string, object>
    {
        // Minimal agent stub backed by an in-memory world state.
        public StubMemory Memory { get; }

        public StubAgent(int moveCount)
        {
            Memory = new StubMemory();
            Memory.GetWorldState().Set("randomMoveCount", moveCount);
        }

        public ReGoap.Core.IReGoapMemory<string, object> GetMemory() => Memory;

        public ReGoap.Core.IReGoapGoal<string, object> GetCurrentGoal() => null;

        public void WarnPossibleGoal(ReGoap.Core.IReGoapGoal<string, object> goal)
        {
        }

        public bool IsActive() => true;

        public System.Collections.Generic.List<ReGoapActionState<string, object>> GetStartingPlan() => new();

        public object GetPlanValue(string key) => null;

        public void SetPlanValue(string key, object value)
        {
        }

        public bool HasPlanValue(string target) => false;

        public System.Collections.Generic.List<ReGoap.Core.IReGoapGoal<string, object>> GetGoalsSet() => new();

        public System.Collections.Generic.List<ReGoap.Core.IReGoapAction<string, object>> GetActionsSet() => new();

        public ReGoapState<string, object> InstantiateNewState() => ReGoapState<string, object>.Instantiate();
    }

    private sealed class StubMemory : ReGoap.Core.IReGoapMemory<string, object>
    {
        private readonly ReGoapState<string, object> worldState = ReGoapState<string, object>.Instantiate();

        public ReGoapState<string, object> GetWorldState() => worldState;
    }
}
#endif
