namespace ReGoap.Godot.FSMExample.Tests;

#if GDUNIT4NET_API_V5
using GdUnit4;

using ReGoap.Core;
using ReGoap.Godot.FSMExample.World;

using static GdUnit4.Assertions;

[TestSuite]
/// <summary>
/// Covers dynamic swordsmith goal target progression behavior.
/// </summary>
public class SwordsmithGoalTests
{
    /// <summary>
    /// Goal target should track chest swords and always ask for one more.
    /// </summary>
    [TestCase]
    [RequireGodotRuntime]
    public void PrecalculationsTargetsCurrentSwordCountPlusOne()
    {
        var goal = new SwordsmithGoal();
        goal._Ready();

        var planner = new StubPlanner(5);
        goal.Precalculations(planner);

        AssertThat(goal.GetGoalState().Get("chestSwordCount")).IsEqual(6);

        planner.SetSwordCount(6);
        goal.Precalculations(planner);
        AssertThat(goal.GetGoalState().Get("chestSwordCount")).IsEqual(7);
    }

    private sealed class StubPlanner : ReGoap.Planner.IGoapPlanner<string, object>
    {
        // Minimal planner stub: only agent access is needed by goal precalculations.
        private readonly StubAgent agent;

        public StubPlanner(int swordCount)
        {
            agent = new StubAgent(swordCount);
        }

        public void SetSwordCount(int swordCount)
        {
            agent.Memory.GetWorldState().Set("chestSwordCount", swordCount);
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

        public StubAgent(int swordCount)
        {
            Memory = new StubMemory();
            Memory.GetWorldState().Set("chestSwordCount", swordCount);
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
