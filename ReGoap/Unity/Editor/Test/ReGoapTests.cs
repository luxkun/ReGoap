using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Planner;
using ReGoap.Unity.Test;
using NUnit.Framework;
using UnityEngine;

namespace ReGoap.Unity.Editor.Test
{
    public class ReGoapTests
    {
        [OneTimeSetUp]
        public void Init()
        {
        }

        [OneTimeTearDown]
        public void Dispose()
        {
        }

        IGoapPlanner<string, object> GetPlanner(bool dynamicActions = false)
        {
            // not using early exit to have precise results, probably wouldn't care in a game for performance reasons
            return new ReGoapPlanner<string, object>(
                new ReGoapPlannerSettings { PlanningEarlyExit = false, UsingDynamicActions = dynamicActions, DebugPlan = true }
            );
        }

        ReGoapTestAgent PrepareAgent(GameObject owner)
        {
            var agent = owner.AddComponent<ReGoapTestAgent>();
            agent.Init();
            return agent;
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
            var state = ReGoapState<string, object>.Instantiate();
            state.Set("var0", true);
            state.Set("var1", "string");
            state.Set("var2", 1);
            var otherState = ReGoapState<string, object>.Instantiate();
            otherState.Set("var1", "stringDifferent");
            otherState.Set("var2", 1);
            var differences = ReGoapState<string, object>.Instantiate();
            var count = state.MissingDifference(otherState, ref differences);
            Assert.That(count, Is.EqualTo(2));
            Assert.That(differences.Get("var0"), Is.EqualTo(true));
            Assert.That(differences.Get("var1"), Is.EqualTo("string"));
            Assert.That(differences.HasKey("var2"), Is.EqualTo(false));
        }

        [Test]
        public void TestReGoapStateAddOperator()
        {
            var state = ReGoapState<string, object>.Instantiate();
            state.Set("var0", true);
            state.Set("var1", "string");
            state.Set("var2", 1);
            var otherState = ReGoapState<string, object>.Instantiate();
            otherState.Set("var2", "new2"); // 2nd one replaces the first
            otherState.Set("var3", true);
            otherState.Set("var4", 10.1f);
            Assert.That(state.Count, Is.EqualTo(3));
            state.AddFromState(otherState);
            Assert.That(otherState.Count, Is.EqualTo(3));
            Assert.That(state.Count, Is.EqualTo(5)); // var2 on first is replaced by var2 on second
            Assert.That(state.Get("var0"), Is.EqualTo(true));
            Assert.That(state.Get("var1"), Is.EqualTo("string"));
            Assert.That(state.Get("var2"), Is.EqualTo("new2"));
            Assert.That(state.Get("var3"), Is.EqualTo(true));
            Assert.That(state.Get("var4"), Is.EqualTo(10.1f));
        }

        [Test]
        public void TestConflictingActionPlan()
        {
            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "JumpIntoWater",
                new Dictionary<string, object> { { "isAtPosition", 0 } },
                new Dictionary<string, object> { { "isAtPosition", 1 }, { "isSwimming", true } }, 1);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GoSwimming",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "isAtPosition", 0 } }, 2);

            var hasAxeGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "Swim",
                new Dictionary<string, object> { { "isSwimming", true } });

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();

            var agent = PrepareAgent(gameObject);

            var plan = GetPlanner().Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(hasAxeGoal));
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, agent, memory);
        }

        public void TestSimpleChainedPlan(IGoapPlanner<string, object> planner)
        {
            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "CreateAxe",
                new Dictionary<string, object> { { "hasWood", true }, { "hasSteel", true } },
                new Dictionary<string, object> { { "hasAxe", true }, { "hasWood", false }, { "hasSteel", false } }, 10);
            ReGoapTestsHelper.GetCustomAction(gameObject, "ChopTree",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "hasRawWood", true } }, 2);
            ReGoapTestsHelper.GetCustomAction(gameObject, "WorksWood",
                new Dictionary<string, object> { { "hasRawWood", true } },
                new Dictionary<string, object> { { "hasWood", true }, { "hasRawWood", false } }, 5);
            ReGoapTestsHelper.GetCustomAction(gameObject, "MineOre",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "hasOre", true } }, 10);
            ReGoapTestsHelper.GetCustomAction(gameObject, "SmeltOre",
                new Dictionary<string, object> { { "hasOre", true } },
                new Dictionary<string, object> { { "hasSteel", true }, { "hasOre", false } }, 10);

            var hasAxeGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "HasAxeGoal",
                new Dictionary<string, object> { { "hasAxe", true } });
            var greedyHasAxeAndOreGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "GreedyHasAxeAndOreGoal",
                new Dictionary<string, object> { { "hasAxe", true }, { "hasOre", true }, { "isGreedy", true } },
                2);

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();

            var agent = PrepareAgent(gameObject);

            var plan = planner.Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(hasAxeGoal));
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, agent, memory);

            // now we set the agent to be greedy, so the second goal can be activated
            memory.SetValue("isGreedy", true);
            // now the planning should choose KillEnemyGoal
            plan = planner.Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(greedyHasAxeAndOreGoal));
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, agent, memory);
        }

        public void TestTwoPhaseChainedPlan(IGoapPlanner<string, object> planner)
        {
            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "CCAction",
                new Dictionary<string, object> { { "hasWeaponEquipped", true }, { "isNearEnemy", true } },
                new Dictionary<string, object> { { "killedEnemy", true } }, 4);
            ReGoapTestsHelper.GetCustomAction(gameObject, "EquipAxe",
                new Dictionary<string, object> { { "hasAxe", true } },
                new Dictionary<string, object> { { "hasWeaponEquipped", true } }, 1);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GoToEnemy",
                new Dictionary<string, object> { { "hasTarget", true } },
                new Dictionary<string, object> { { "isNearEnemy", true } }, 3);
            ReGoapTestsHelper.GetCustomAction(gameObject, "CreateAxe",
                new Dictionary<string, object> { { "hasWood", true }, { "hasSteel", true } },
                new Dictionary<string, object> { { "hasAxe", true }, { "hasWood", false }, { "hasSteel", false } }, 10);
            ReGoapTestsHelper.GetCustomAction(gameObject, "ChopTree",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "hasRawWood", true } }, 2);
            ReGoapTestsHelper.GetCustomAction(gameObject, "WorksWood",
                new Dictionary<string, object> { { "hasRawWood", true } },
                new Dictionary<string, object> { { "hasWood", true }, { "hasRawWood", false } }, 5);
            ReGoapTestsHelper.GetCustomAction(gameObject, "MineOre", new Dictionary<string, object> { },
                new Dictionary<string, object> { { "hasOre", true } }, 10);
            ReGoapTestsHelper.GetCustomAction(gameObject, "SmeltOre",
                new Dictionary<string, object> { { "hasOre", true } },
                new Dictionary<string, object> { { "hasSteel", true }, { "hasOre", false } }, 10);

            var readyToFightGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "ReadyToFightGoal",
                new Dictionary<string, object> { { "hasWeaponEquipped", true } }, 2);
            ReGoapTestsHelper.GetCustomGoal(gameObject, "HasAxeGoal",
                new Dictionary<string, object> { { "hasAxe", true } });
            var killEnemyGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "KillEnemyGoal",
                new Dictionary<string, object> { { "killedEnemy", true } }, 3);

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();

            var agent = PrepareAgent(gameObject);


            // first plan should create axe and equip it, through 'ReadyToFightGoal', since 'hasTarget' is false (memory should handle this)
            var plan = planner.Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(readyToFightGoal));
            // we apply manually the effects, but in reality the actions should do this themselves 
            //  and the memory should understand what happened 
            //  (e.g. equip weapon action? memory should set 'hasWeaponEquipped' to true if the action equipped something)
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, agent, memory);

            // now we tell the memory that we see the enemy
            memory.SetValue("hasTarget", true);
            // now the planning should choose KillEnemyGoal
            plan = planner.Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(killEnemyGoal));
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, agent, memory);
        }

        // Additional test by TPMxyz
        [Test]
        public void TestGatherGotoGather()
        {
            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "GatherApple",
                new Dictionary<string, object> { { "At", "Farm" } },
                new Dictionary<string, object> { { "hasApple", true } }, 1);
            ReGoapTestsHelper.GetCustomAction(gameObject, "GatherPeach",
                new Dictionary<string, object> { { "At", "Farm" } },
                new Dictionary<string, object> { { "hasPeach", true } }, 2);
            ReGoapTestsHelper.GetCustomAction(gameObject, "Goto",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "At", "Farm" } }, 10);

            var theGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "GatherAll",
                new Dictionary<string, object> { { "hasApple", true }, { "hasPeach", true } });

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();

            var plan = GetPlanner().Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(theGoal));
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, agent, memory);
        }

        // Additional test by TPMxyz
        [Test]
        public void TestActionOverrideGoal()
        {
            var gameObject = new GameObject();

            ReGoapTestsHelper.GetCustomAction(gameObject, "Mine Ore",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "hasMoney", true } }, 10);
            ReGoapTestsHelper.GetCustomAction(gameObject, "Buy Food",
                new Dictionary<string, object> { { "hasMoney", true } },
                new Dictionary<string, object> { { "hasFood", true }, { "hasMoney", false } }, 2);

            var theGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "PrepareFoodAndMoney",
                new Dictionary<string, object> { { "hasMoney", true }, { "hasFood", true } });

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();

            var plan = GetPlanner().Plan(agent, null, null, null);

            Assert.That(plan, Is.EqualTo(theGoal));
            // validate plan actions
            ReGoapTestsHelper.ApplyAndValidatePlan(plan, agent, memory);
        }

        [Test]
        public void TestDynamicAction()
        {
            var gameObject = new GameObject();

            // settings these value to make sure that the planning chooses weaponA, since the pathing
            // weaponA -> ammoA -> enemy is actually cheaper, even if weaponC is very close to the enemy
            // without dynamic cost weaponC would always be chosen
            // we also add weaponB and ammoB to show the reconcileStartPosition logic
            // with this requirement in the goal and this action, we push as cost a goto to the player position
            // this effectively makes the plan: weaponB -> ammoB -> enemy more expensive, without reconciling with the starting position this plan would have been the best.
            var playerPosition = Vector2.zero;
            var enemyPosition = new Vector2(0, 100);
            var weaponAPosition = new Vector2(0, 50);
            var ammoAPosition = new Vector2(0, 60);
            var weaponBPosition = new Vector2(0, 115);
            var ammoBPosition = new Vector2(0, 115);
            var weaponCPosition = new Vector2(-5, 100);
            var weaponRange = 20.0f;

            ReGoapTestsHelper.GetCustomAction(gameObject, "ShootEnemy",
                new Dictionary<string, object> {
                    { "weaponReady", true }, { "isAt", enemyPosition }, { "inRange", weaponRange } },
                new Dictionary<string, object> { { "shootEnemy", true } }, 100);
            ReGoapTestsHelper.GetCustomAction(gameObject, "ReloadWeapon",
                new Dictionary<string, object> { { "hasWeapon", true }, { "hasAmmo", true } },
                new Dictionary<string, object> { { "weaponReady", true } }, 20);
            #region getWeapon
            var getWeapon = ReGoapTestsHelper.GetCustomAction(gameObject, "GetWeapon",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "hasWeapon", true } }, 5);
            getWeapon.CustomPreconditionsGetter = (ref ReGoapState<string, object> preconditions, GoapActionStackData<string, object> stackData) =>
            {
                preconditions.Clear();
                if (stackData.settings.HasKey("weaponPosition"))
                {
                    preconditions.Set("isAt", (Vector2)stackData.settings.Get("weaponPosition"));
                }
            };
            getWeapon.CustomEffectsGetter = (ref ReGoapState<string, object> effects, GoapActionStackData<string, object> stackData) =>
            {
                effects.Clear();
                if (stackData.settings.HasKey("weaponPosition"))
                {
                    effects.Set("hasWeapon", true);
                }
            };
            getWeapon.CustomSettingsGetter = (GoapActionStackData<string, object> stackData) =>
            {
                var results = new List<ReGoapState<string, object>>();

                if (stackData.currentState.HasKey("weaponPositions") && stackData.currentState.HasKey("isAt"))
                {
                    var currentPosition = (Vector2)stackData.currentState.Get("isAt");

                    foreach (var objectPosition in (List<Vector2>)stackData.currentState.Get("weaponPositions"))
                    {
                        ReGoapState<string, object> settings = ReGoapState<string, object>.Instantiate();
                        settings.Set("weaponPosition", objectPosition);
                        results.Add(settings);
                    }
                }
                return results;
            };
            #endregion
            #region getAmmo
            var getAmmo = ReGoapTestsHelper.GetCustomAction(gameObject, "GetAmmo",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { { "hasAmmo", true } }, 3);

            getAmmo.CustomPreconditionsGetter = (ref ReGoapState<string, object> preconditions, GoapActionStackData<string, object> stackData) =>
            {
                preconditions.Clear();
                if (stackData.settings.HasKey("ammoPosition"))
                {
                    preconditions.Set("isAt", (Vector2)stackData.settings.Get("ammoPosition"));
                }
            };
            getAmmo.CustomEffectsGetter = (ref ReGoapState<string, object> effects, GoapActionStackData<string, object> stackData) =>
            {
                effects.Clear();
                if (stackData.settings.HasKey("ammoPosition"))
                {
                    effects.Set("hasAmmo", true);
                }
            };
            getAmmo.CustomSettingsGetter = (GoapActionStackData<string, object> stackData) =>
            {
                var results = new List<ReGoapState<string, object>>();

                if (stackData.currentState.HasKey("ammoPositions") && stackData.currentState.HasKey("isAt"))
                {
                    var currentPosition = (Vector2)stackData.currentState.Get("isAt");

                    foreach (var objectPosition in (List<Vector2>)stackData.currentState.Get("ammoPositions"))
                    {
                        ReGoapState<string, object> settings = ReGoapState<string, object>.Instantiate();
                        settings.Set("ammoPosition", objectPosition);
                        results.Add(settings);
                    }
                }
                return results;
            };
            #endregion
            #region dynamicGoTo
            var dynamicGoTo = ReGoapTestsHelper.GetCustomAction(gameObject, "GoTo",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { });
            dynamicGoTo.CustomCostGetter = (ref float cost, GoapActionStackData<string, object> stackData) =>
            {
                // base value to avoid free action
                cost = 1.0f;
                var inRange = 0.0f;
                if (stackData.settings.HasKey("inRange"))
                {
                    inRange = (float)stackData.settings.Get("inRange");
                }
                if (stackData.settings.HasKey("isAt") && stackData.currentState.HasKey("isAt"))
                {
                    var wantedPosition = (Vector2)stackData.settings.Get("isAt");
                    var currentPosition = (Vector2)stackData.currentState.Get("isAt");
                    cost = (wantedPosition - currentPosition).magnitude - inRange;
                    if (cost < 0) cost = 0;
                }
            };
            dynamicGoTo.CustomEffectsGetter = (ref ReGoapState<string, object> effects, GoapActionStackData<string, object> stackData) =>
            {
                effects.Clear();
                if (stackData.settings.HasKey("isAt"))
                {
                    var wantedPosition = (Vector2)stackData.settings.Get("isAt");
                    effects.Set("isAt", wantedPosition);
                }
                if (stackData.settings.HasKey("inRange"))
                {
                    var inRange = (float)stackData.settings.Get("inRange");
                    effects.Set("inRange", inRange);
                }
            };
            dynamicGoTo.CustomSettingsGetter = (GoapActionStackData<string, object> stackData) =>
            {
                var newSettings = ReGoapState<string, object>.Instantiate();

                Vector2 wantedPosition = Vector2.zero;
                float inRange = 0.0f;
                if (stackData.goalState.HasKey("isAt"))
                {
                    wantedPosition = (Vector2)stackData.goalState.Get("isAt");
                }
                if (stackData.goalState.HasKey("inRange"))
                {
                    inRange = (float)stackData.goalState.Get("inRange");
                }
                newSettings.Set("isAt", wantedPosition);
                newSettings.Set("inRange", inRange);
                return new List<ReGoapState<string, object>> { newSettings };
            };
            #endregion
            #region reconcileStartPosition
            var reconcileStartPosition = ReGoapTestsHelper.GetCustomAction(gameObject, "ReconcileStartPosition",
                new Dictionary<string, object> { },
                new Dictionary<string, object> { }, 1);
            reconcileStartPosition.CustomPreconditionsGetter = (ref ReGoapState<string, object> preconditions, GoapActionStackData<string, object> stackData) =>
            {
                preconditions.Clear();
                // this could be fetched from the world memory, in a custom action class
                preconditions.Set("isAt", playerPosition);
            };
            reconcileStartPosition.CustomEffectsGetter = (ref ReGoapState<string, object> effects, GoapActionStackData<string, object> stackData) =>
            {
                effects.Clear();
                // we want this action to work only if no other goal has to be archived
                if (stackData.goalState.HasKey("reconcileStartPosition") && stackData.goalState.Count == 1)
                {
                    effects.Set("reconcileStartPosition", true);
                }
            };
            #endregion

            var theGoal = ReGoapTestsHelper.GetCustomGoal(gameObject, "ShootEnemy",
                new Dictionary<string, object> { { "shootEnemy", true }, { "reconcileStartPosition", true } });

            var memory = gameObject.AddComponent<ReGoapTestMemory>();
            memory.Init();
            memory.SetValue("enemyPosition", enemyPosition);
            memory.SetValue("ammoPositions", new List<Vector2> { ammoAPosition, ammoBPosition });
            memory.SetValue("weaponPositions", new List<Vector2> { weaponAPosition, weaponBPosition, weaponCPosition });

            var agent = gameObject.AddComponent<ReGoapTestAgent>();
            agent.Init();

            var plan = GetPlanner(dynamicActions: true).Plan(agent, null, null, null);
        }
    }
}