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
    }

    public class MyMemory : GoapMemory
    {
        public void Init()
        {
            Awake();
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
        planner = new ReGoapPlanner();
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

    private MyGoal GetCustomGoal(GameObject gameObject, string name, Dictionary<string, bool> goalState)
    {
        var customGoal = gameObject.AddComponent<MyGoal>();
        customGoal.Name = name;
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
    public void TestCreateAxePlan()
    {
        var gameObject = new GameObject();

        var createAxeAction = GetCustomAction(gameObject, "CreateAxe",
            new Dictionary<string, bool> {{"hasAxe", false}, {"hasWood", true}, {"hasSteel", true}},
            new Dictionary<string, bool> {{"hasAxe", true}, {"hasWood", false}, {"hasSteel", false}}, 5);
        var chopTreeAction = GetCustomAction(gameObject, "ChopTree",
            new Dictionary<string, bool> {{"hasRawWood", false}}, new Dictionary<string, bool> {{"hasRawWood", true}});
        var worksWoodAction = GetCustomAction(gameObject, "WorksWood",
            new Dictionary<string, bool> {{"hasWood", false}, {"hasRawWood", true}},
            new Dictionary<string, bool> {{"hasWood", true}, {"hasRawWood", false}}, 2);
        var mineOreAction = GetCustomAction(gameObject, "MineOre", new Dictionary<string, bool> {{"hasOre", false}},
            new Dictionary<string, bool> {{"hasOre", true}}, 3);
        var smeltOreAction = GetCustomAction(gameObject, "SmeltOre",
            new Dictionary<string, bool> {{"hasOre", true}, {"hasSteel", false}},
            new Dictionary<string, bool> {{"hasSteel", true}, {"hasOre", false}}, 4);

        var hasAxeGoal = GetCustomGoal(gameObject, "HasAxeGoal", new Dictionary<string, bool> { { "hasAxe", true } });

        var memory = gameObject.AddComponent<MyMemory>();
        memory.Init();

        var agent = gameObject.AddComponent<MyAgent>();
        agent.Init();

        var plan = planner.Plan(agent);
        var expectedPlan = new Queue<IReGoapAction>();
        expectedPlan.Enqueue(chopTreeAction);
        expectedPlan.Enqueue(worksWoodAction);
        expectedPlan.Enqueue(mineOreAction);
        expectedPlan.Enqueue(smeltOreAction);
        expectedPlan.Enqueue(createAxeAction);

        Assert.That(plan, Is.EqualTo(hasAxeGoal));
        Assert.That(plan.GetPlan(), Is.EqualTo(expectedPlan));
    }
}
