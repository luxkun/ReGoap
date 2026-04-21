# ReGoap

ReGoap is a generic C# GOAP (Goal Oriented Action Planning) library with Unity and Godot adapters.

It gives you a planner that chooses a goal, builds a valid action sequence to satisfy it, and lets your runtime execute that sequence while reacting to world changes.

## What GOAP is

GOAP is a decision-making technique for agents (NPCs, workers, enemies, simulation entities) where behavior is generated from world facts instead of hardcoded behavior trees or giant switch statements.

In GOAP, you define:

- **World state** facts (example: `hasOre = true`, `chestSwordCount = 3`)
- **Actions** with preconditions and effects (example: `SmeltIngot` needs ore and produces ingot)
- **Goals** as desired world states (example: `chestSwordCount >= 10`)

At runtime, the planner searches for a valid chain of actions that transforms the current world state into the goal state.

Why this is useful:

- You add/modify actions and goals without rewriting all transitions.
- Agents can recover from changed world conditions via replanning.
- The system naturally supports emergent sequences from reusable actions.

## What ReGoap does

ReGoap provides:

- A reusable, engine-agnostic planner core (`ReGoap/Core`, `ReGoap/Planner`, `ReGoap/Utilities`)
- Runtime abstractions for agent, action, goal, memory, and sensors
- Engine adapters (Unity and Godot) so you can plug into scene/component workflows
- Debugging support (notably in Godot runtime debugger)

At a high level, each planning cycle is:

1. Agent gathers possible goals.
2. Planner picks a goal (deterministic or weighted-random mode).
3. Planner runs A* search to build a valid action plan.
4. Agent executes actions in order.
5. On success/failure/world change, agent can re-evaluate and replan.

## Core concepts

### State

A key/value fact container describing world conditions.

Examples:

- `enemyVisible = true`
- `weaponEquipped = false`
- `chestOreCount = 2`

### Action

A behavior unit with:

- Preconditions: what must be true to run it
- Effects: what it contributes toward goals
- Cost: planning cost used by search
- Runtime `Run` logic: your actual gameplay/sim code

### Goal

A desired target state plus priority.

The planner chooses among possible goals and attempts to build a valid plan for one.

### Memory

The agent's current world knowledge.

This is the source-of-truth state the planner reads when generating plans.

### Sensor

A memory updater that writes observed world facts into memory (resource counts, visibility, occupancy, etc).

## New/global planner features

### Comparator conditions

ReGoap supports comparator-based state matching through `ReGoapCondition`:

- `ReGoapCondition.Equal(value)`
- `ReGoapCondition.NotEqual(value)`
- `ReGoapCondition.GreaterOrEqual(value)`
- `ReGoapCondition.LessOrEqual(value)`

This is useful for count-based logic such as inventories, chest resources, cooldown thresholds, and score targets.

Plain values still use exact equality matching for backward compatibility.

### Weighted-random goal selection

Goal selection can be deterministic (highest priority first) or weighted-random (priority-biased random).

Settings in `ReGoapPlannerSettings`:

- `UseWeightedRandomGoalSelection` (default `false`)
- `WeightedRandomGoalPriorityPower` (default `1f`)
- `WeightedRandomMinimumWeight` (default `0.001f`)
- `WeightedRandomUseDeterministicSeed` (default `false`)
- `WeightedRandomSeed` (default `0`)

Deterministic seed mode is helpful for reproducible tests/replays.

## Repository layout

- `ReGoap/Core`: interfaces, states, action/goal contracts
- `ReGoap/Planner`: planner, nodes, settings
- `ReGoap/Utilities`: logging and utility helpers
- `ReGoap/Unity`: Unity adapter + Unity examples/docs
- `ReGoap/Godot`: Godot adapter + Godot FSMExample/docs

## Engine-specific guides

- Unity guide: `ReGoap/Unity/README.md`
- Godot guide: `ReGoap/Godot/README.md`

## Typical integration flow

1. Define your world facts in memory/sensors.
2. Implement actions with realistic preconditions/effects/costs.
3. Implement goals with priorities.
4. Attach agent + planner manager.
5. Validate behavior in debugger and with tests.
6. Tune costs/priorities to shape behavior.

## Contributing

PRs are welcome. Run relevant build/tests for changed engine/runtime areas before submitting.
