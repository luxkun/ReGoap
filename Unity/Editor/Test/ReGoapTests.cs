using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;

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

    IGoapPlanner GetPlanner(bool backward)
    {
        // not using early exit to have precise results, probably wouldn't care in a game for performance reasons
        return new ReGoapPlanner(
            new ReGoapPlannerSettings { planningEarlyExit = false, backwardSearch = backward }
        );
    }

    private ReGoapTestsHelper.MyAction GetCustomAction(GameObject gameObject, string name, Dictionary<string, bool> preconditionsBools,
        Dictionary<string, bool> effectsBools, int cost = 1)
    {
        var effects = new ReGoapState();
        var preconditions = new ReGoapState();
        var customAction = gameObject.AddComponent<ReGoapTestsHelper.MyAction>();
        customAction.Name = name;
        customAction.Init();
        foreach (var pair in effectsBools)
            effects.Set(pair.Key, pair.Value);
        customAction.SetEffects(effects);
        foreach (var pair in preconditionsBools)
            preconditions.Set(pair.Key, pair.Value);
        customAction.SetPreconditions(preconditions);
        customAction.cost = cost;
        return customAction;
    }

    private ReGoapTestsHelper.MyGoal GetCustomGoal(GameObject gameObject, string name, Dictionary<string, bool> goalState, int priority = 1)
    {
        var customGoal = gameObject.AddComponent<ReGoapTestsHelper.MyGoal>();
        customGoal.Name = name;
        customGoal.SetPriority(priority);
        customGoal.Init();
        var goal = new ReGoapState();
        foreach (var pair in goalState)
        {
            goal.Set(pair.Key, pair.Value);
        }
        customGoal.SetGoalState(goal);
        return customGoal;
    }

    [Test]
    public void TestSimpleChainedPlanForward()
    {
        TestSimpleChainedPlan(GetPlanner(false));
    }
    [Test]
    public void TestSimpleChainedPlanBackward()
    {
        TestSimpleChainedPlan(GetPlanner(true));
    }

    [Test]
    public void TestTwoPhaseChainedPlanForward()
    {
        TestTwoPhaseChainedPlan(GetPlanner(false));
    }
    [Test]
    public void TestTwoPhaseChainedPlanBackward()
    {
        TestTwoPhaseChainedPlan(GetPlanner(true));
    }

    public void TestSimpleChainedPlan(IGoapPlanner planner)
    {
        var gameObject = new GameObject();

        var createAxeAction = GetCustomAction(gameObject, "CreateAxe",
            new Dictionary<string, bool> { { "hasAxe", false }, { "hasWood", true }, { "hasSteel", true } },
            new Dictionary<string, bool> { { "hasAxe", true }, { "hasWood", false }, { "hasSteel", false } }, 10);
        var chopTreeAction = GetCustomAction(gameObject, "ChopTree",
            new Dictionary<string, bool> { { "hasRawWood", false } },
            new Dictionary<string, bool> { { "hasRawWood", true } }, 2);
        var worksWoodAction = GetCustomAction(gameObject, "WorksWood",
            new Dictionary<string, bool> { { "hasWood", false }, { "hasRawWood", true } },
            new Dictionary<string, bool> { { "hasWood", true }, { "hasRawWood", false } }, 5);
        var mineOreAction = GetCustomAction(gameObject, "MineOre",
            new Dictionary<string, bool> { { "hasOre", false } },
            new Dictionary<string, bool> { { "hasOre", true } }, 10);
        var smeltOreAction = GetCustomAction(gameObject, "SmeltOre",
            new Dictionary<string, bool> { { "hasOre", true }, { "hasSteel", false } },
            new Dictionary<string, bool> { { "hasSteel", true }, { "hasOre", false } }, 10);

        var hasAxeGoal = GetCustomGoal(gameObject, "HasAxeGoal", new Dictionary<string, bool> { { "hasAxe", true } });

        var memory = gameObject.AddComponent<ReGoapTestsHelper.MyMemory>();
        memory.Init();

        var agent = gameObject.AddComponent<ReGoapTestsHelper.MyAgent>();
        agent.Init();

        var plan = planner.Plan(agent, null, null, null);

        Assert.That(plan, Is.EqualTo(hasAxeGoal));
        // validate plan actions
        ReGoapTestsHelper.ApplyAndValidatePlan(plan, memory);
    }

    public void TestTwoPhaseChainedPlan(IGoapPlanner planner)
    {
        var gameObject = new GameObject();

        var closeCombatAction = GetCustomAction(gameObject, "CCAction",
            new Dictionary<string, bool> { { "hasWeaponEquipped", true }, { "killedEnemy", false }, { "isNearEnemy", true } },
            new Dictionary<string, bool> { { "killedEnemy", true } }, 4);
        var equipAxe = GetCustomAction(gameObject, "EquipAxe",
            new Dictionary<string, bool> { { "hasAxe", true }, { "hasWeaponEquipped", false } },
            new Dictionary<string, bool> { { "hasWeaponEquipped", true } }, 1);
        var goToEnemy = GetCustomAction(gameObject, "GoToEnemy",
            new Dictionary<string, bool> { { "isNearEnemy", false }, { "hasTarget", true } },
            new Dictionary<string, bool> { { "isNearEnemy", true } }, 3);
        var createAxeAction = GetCustomAction(gameObject, "CreateAxe",
            new Dictionary<string, bool> { { "hasAxe", false }, { "hasWood", true }, { "hasSteel", true } },
            new Dictionary<string, bool> { { "hasAxe", true }, { "hasWood", false }, { "hasSteel", false } }, 10);
        var chopTreeAction = GetCustomAction(gameObject, "ChopTree",
            new Dictionary<string, bool> { { "hasRawWood", false } }, new Dictionary<string, bool> { { "hasRawWood", true } }, 2);
        var worksWoodAction = GetCustomAction(gameObject, "WorksWood",
            new Dictionary<string, bool> { { "hasWood", false }, { "hasRawWood", true } },
            new Dictionary<string, bool> { { "hasWood", true }, { "hasRawWood", false } }, 5);
        var mineOreAction = GetCustomAction(gameObject, "MineOre", new Dictionary<string, bool> { { "hasOre", false } },
            new Dictionary<string, bool> { { "hasOre", true } }, 10);
        var smeltOreAction = GetCustomAction(gameObject, "SmeltOre",
            new Dictionary<string, bool> { { "hasOre", true }, { "hasSteel", false } },
            new Dictionary<string, bool> { { "hasSteel", true }, { "hasOre", false } }, 10);

        var readyToFightGoal = GetCustomGoal(gameObject, "ReadyToFightGoal", new Dictionary<string, bool> { { "hasWeaponEquipped", true } }, 2);
        var hasAxeGoal = GetCustomGoal(gameObject, "HasAxeGoal", new Dictionary<string, bool> { { "hasAxe", true } });
        var killEnemyGoal = GetCustomGoal(gameObject, "KillEnemyGoal", new Dictionary<string, bool> { { "killedEnemy", true } }, 3);

        var memory = gameObject.AddComponent<ReGoapTestsHelper.MyMemory>();
        memory.Init();

        var agent = gameObject.AddComponent<ReGoapTestsHelper.MyAgent>();
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
