using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class ReGoapTests
{
    public class MyAction : GoapAction
    {
        public void Init()
        {
            Awake();
        }

        public void SetEffects(ReGoapState effects)
        {
            this.effects = effects;
        }

        public void SetPreconditions(ReGoapState preconditions)
        {
            this.preconditions = preconditions;
        }
    }

    public class MyGoal : GoapGoal
    {
        public void Init()
        {
            Awake();
        }

        public void SetGoalState(ReGoapState goalState)
        {
            goal = goalState;
        }

        public void SetPriority(int priority)
        {
            this.priority = priority;
        }
    }

    public class MyMemory : GoapMemory
    {
        public void Init()
        {
            Awake();
        }

        public void SetValue<T>(string key, T value)
        {
            state.Set(key, value);
        }
    }

    public class MyAgent : GoapAgent
    {
        public void Init()
        {
            Awake();
        }
    }

    private ReGoapPlanner planner;

    [TestFixtureSetUp]
    public void Init()
    {
        // not using early exit to have precise results, probably wouldn't care in a game for performance reasons
        planner = new ReGoapPlanner(new ReGoapPlannerSettings { planningEarlyExit = false });
    }

    [TestFixtureTearDown]
    public void Dispose()
    {
    }

    private MyAction GetCustomAction(GameObject gameObject, string name, Dictionary<string, bool> preconditionsBools,
        Dictionary<string, bool> effectsBools, int cost = 1)
    {
        var effects = new ReGoapState();
        var preconditions = new ReGoapState();
        var customAction = gameObject.AddComponent<MyAction>();
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

    private MyGoal GetCustomGoal(GameObject gameObject, string name, Dictionary<string, bool> goalState, int priority = 1)
    {
        var customGoal = gameObject.AddComponent<MyGoal>();
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

    private void ApplyAndValidatePlan(IReGoapGoal plan, MyMemory memory)
    {
        foreach (var action in plan.GetPlan())
        {
            Assert.That(action.GetPreconditions(plan.GetGoalState()).MissingDifference(memory.GetWorldState(), 1) == 0);
            foreach (var effectsPair in action.GetEffects(plan.GetGoalState()).GetValues())
            {   // in a real game this should be done by memory itself
                //  e.x. isNearTarget = (transform.position - target.position).magnitude < minRangeForCC
                memory.SetValue(effectsPair.Key, effectsPair.Value);
            }
        }
        Assert.That(plan.GetGoalState().MissingDifference(memory.GetWorldState(), 1) == 0);
    }

    [Test]
    public void TestSimpleChainedPlan()
    {
        var gameObject = new GameObject();

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

        var hasAxeGoal = GetCustomGoal(gameObject, "HasAxeGoal", new Dictionary<string, bool> { { "hasAxe", true } });

        var memory = gameObject.AddComponent<MyMemory>();
        memory.Init();

        var agent = gameObject.AddComponent<MyAgent>();
        agent.Init();

        var plan = planner.Plan(agent);

        Assert.That(plan, Is.EqualTo(hasAxeGoal));
        // validate plan actions
        ApplyAndValidatePlan(plan, memory);
    }

    [Test]
    public void TestTwoPhaseChainedPlan()
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

        var memory = gameObject.AddComponent<MyMemory>();
        memory.Init();

        var agent = gameObject.AddComponent<MyAgent>();
        agent.Init();

        // first plan should create axe and equip it, through 'ReadyToFightGoal', since 'hasTarget' is false (memory should handle this)
        var plan = planner.Plan(agent);

        Assert.That(plan, Is.EqualTo(readyToFightGoal));
        // we apply manually the effects, but in reality the actions should do this themselves 
        //  and the memory should understand what happened 
        //  (e.g. equip weapon action? memory should set 'hasWeaponEquipped' to true if the action equipped something)
        // validate plan actions
        ApplyAndValidatePlan(plan, memory);

        // now we tell the memory that we see the enemy
        memory.SetValue("hasTarget", true);
        // now the planning should choose KillEnemyGoal
        plan = planner.Plan(agent);

        Assert.That(plan, Is.EqualTo(killEnemyGoal));
        ApplyAndValidatePlan(plan, memory);
    }
}
