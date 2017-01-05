using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class ReGoapTests
{
    [TestFixtureSetUp]
    public void Init()
    {
    }

    [TestFixtureTearDown]
    public void Dispose()
    {
    }

    IGoapPlanner GetPlanner()
    {
        // not using early exit to have precise results, probably wouldn't care in a game for performance reasons
        return new ReGoapPlanner(
            new ReGoapPlannerSettings {PlanningEarlyExit = false}
        );
    }

    [Test]
    public void TestSimpleChainedPlan()
    {
        TestSimpleChainedPlan(GetPlanner());
    }

    [Test]
    public void TestTwoPhaseChainedPlan()
    {
        TestTwoPhaseChainedPlan(GetPlanner());
    }

    [Test]
    public void TestReGoapStateMissingDifference()
    {
        var state = new ReGoapState();
        state.Set("var0", true);
        state.Set("var1", "string");
        state.Set("var2", 1);
        var otherState = new ReGoapState();
        otherState.Set("var1", "stringDifferent");
        otherState.Set("var2", 1);
        var differences = new ReGoapState();
        var count = state.MissingDifference(otherState, ref differences);
        Assert.That(count, Is.EqualTo(2));
        Assert.That(differences.Get<bool>("var0"), Is.EqualTo(true));
        Assert.That(differences.Get<string>("var1"), Is.EqualTo("string"));
        Assert.That(differences.HasKey("var2"), Is.EqualTo(false));
    }

    [Test]
    public void TestReGoapStateAddOperator()
    {
        var state = new ReGoapState();
        state.Set("var0", true);
        state.Set("var1", "string");
        state.Set("var2", 1);
        var otherState = new ReGoapState();
        otherState.Set("var2", "new2"); // 2nd one replaces the first
        otherState.Set("var3", true);
        otherState.Set("var4", 10.1f);
        var sum = state + otherState;
        Assert.That(state.Count + otherState.Count, Is.EqualTo(6));
        Assert.That(sum.Count, Is.EqualTo(5)); // var2 on first is replaced by var2 on second
        Assert.That(sum.Get<bool>("var0"), Is.EqualTo(true));
        Assert.That(sum.Get<string>("var1"), Is.EqualTo("string"));
        Assert.That(sum.Get<string>("var2"), Is.EqualTo("new2"));
        Assert.That(sum.Get<bool>("var3"), Is.EqualTo(true));
        Assert.That(sum.Get<float>("var4"), Is.EqualTo(10.1f));
    }

    public void TestSimpleChainedPlan(IGoapPlanner planner)
    {
        var gameObject = new GameObject();

        ReGoapTestsHelper.GetCustomAction(gameObject, "CreateAxe",
            new Dictionary<string, bool> {{"hasWood", true}, {"hasSteel", true}},
            new Dictionary<string, bool> {{"hasAxe", true}, {"hasWood", false}, {"hasSteel", false}}, 10);
        ReGoapTestsHelper.GetCustomAction(gameObject, "ChopTree",
            new Dictionary<string, bool> {},
            new Dictionary<string, bool> {{"hasRawWood", true}}, 2);
        ReGoapTestsHelper.GetCustomAction(gameObject, "WorksWood",
            new Dictionary<string, bool> {{"hasRawWood", true}},
            new Dictionary<string, bool> {{"hasWood", true}, {"hasRawWood", false}}, 5);
        ReGoapTestsHelper.GetCustomAction(gameObject, "MineOre",
            new Dictionary<string, bool> {{"hasOre", false}},
            new Dictionary<string, bool> {{"hasOre", true}}, 10);
        ReGoapTestsHelper.GetCustomAction(gameObject, "SmeltOre",
            new Dictionary<string, bool> {{"hasOre", true}, {"hasSteel", false}},
            new Dictionary<string, bool> {{"hasSteel", true}, {"hasOre", false}}, 10);

        var hasAxeGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "HasAxeGoal",
            new Dictionary<string, bool> {{"hasAxe", true}});

        var memory = gameObject.AddComponent<ReGoapTestMemory>();
        memory.Init();

        var agent = gameObject.AddComponent<ReGoapTestAgent>();
        agent.Init();

        var plan = planner.Plan(agent, null, null, null);

        Assert.That(plan, Is.EqualTo(hasAxeGoal));
        // validate plan actions
        ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
    }

    public void TestTwoPhaseChainedPlan(IGoapPlanner planner)
    {
        var gameObject = new GameObject();

        ReGoapTestsHelper.GetCustomAction(gameObject, "CCAction",
            new Dictionary<string, bool> {{"hasWeaponEquipped", true}, {"isNearEnemy", true}},
            new Dictionary<string, bool> {{"killedEnemy", true}}, 4);
        ReGoapTestsHelper.GetCustomAction(gameObject, "EquipAxe",
            new Dictionary<string, bool> {{"hasAxe", true}},
            new Dictionary<string, bool> {{"hasWeaponEquipped", true}}, 1);
        ReGoapTestsHelper.GetCustomAction(gameObject, "GoToEnemy",
            new Dictionary<string, bool> {{"hasTarget", true}},
            new Dictionary<string, bool> {{"isNearEnemy", true}}, 3);
        ReGoapTestsHelper.GetCustomAction(gameObject, "CreateAxe",
            new Dictionary<string, bool> {{"hasWood", true}, {"hasSteel", true}},
            new Dictionary<string, bool> {{"hasAxe", true}, {"hasWood", false}, {"hasSteel", false}}, 10);
        ReGoapTestsHelper.GetCustomAction(gameObject, "ChopTree",
            new Dictionary<string, bool> {}, 
            new Dictionary<string, bool> {{"hasRawWood", true}}, 2);
        ReGoapTestsHelper.GetCustomAction(gameObject, "WorksWood",
            new Dictionary<string, bool> {{"hasRawWood", true}},
            new Dictionary<string, bool> {{"hasWood", true}, {"hasRawWood", false}}, 5);
        ReGoapTestsHelper.GetCustomAction(gameObject, "MineOre", new Dictionary<string, bool> {{"hasOre", false}},
            new Dictionary<string, bool> {{"hasOre", true}}, 10);
        ReGoapTestsHelper.GetCustomAction(gameObject, "SmeltOre",
            new Dictionary<string, bool> {{"hasOre", true}},
            new Dictionary<string, bool> {{"hasSteel", true}, {"hasOre", false}}, 10);

        var readyToFightGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "ReadyToFightGoal",
            new Dictionary<string, bool> {{"hasWeaponEquipped", true}}, 2);
        ReGoapTestsHelper.GetCustomGoal(gameObject, "HasAxeGoal",
            new Dictionary<string, bool> {{"hasAxe", true}});
        var killEnemyGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "KillEnemyGoal",
            new Dictionary<string, bool> {{"killedEnemy", true}}, 3);

        var memory = gameObject.AddComponent<ReGoapTestMemory>();
        memory.Init();

        var agent = gameObject.AddComponent<ReGoapTestAgent>();
        agent.Init();

        // first plan should create axe and equip it, through 'ReadyToFightGoal', since 'hasTarget' is false (memory should handle this)
        var plan = planner.Plan(agent, null, null, null);

        Assert.That(plan, Is.EqualTo(readyToFightGoal));
        // we apply manually the effects, but in reality the actions should do this themselves 
        //  and the memory should understand what happened 
        //  (e.g. equip weapon action? memory should set 'hasWeaponEquipped' to true if the action equipped something)
        // validate plan actions
        ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);

        // now we tell the memory that we see the enemy
        memory.SetValue("hasTarget", true);
        // now the planning should choose KillEnemyGoal
        plan = planner.Plan(agent, null, null, null);

        Assert.That(plan, Is.EqualTo(killEnemyGoal));
        ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
    }
}