# ReGoap Godot

## What is in this folder

Godot adapter base classes:

- `ReGoapAction<T, W>`
- `ReGoapAgent<T, W>` / `ReGoapAgentAdvanced<T, W>`
- `ReGoapGoal<T, W>` / `ReGoapGoalAdvanced<T, W>`
- `ReGoapMemory<T, W>` / `ReGoapMemoryAdvanced<T, W>`
- `ReGoapSensor<T, W>`
- `ReGoapPlannerManager<T, W>`
- `ReGoapDebugger`

Shared Godot runtime project:

- `ReGoap/Godot/regoap_godot_runtime.csproj`

Example project:

- `ReGoap/Godot/FSMExample/project.godot`
- Main scene: `ReGoap/Godot/FSMExample/Scenes/TestMap.tscn`

## Recommended integration for your own Godot game

Use the shared runtime project as a reference.

1. Include `ReGoap/Godot/regoap_godot_runtime.csproj` in your solution.
2. Add a project reference from your game `.csproj` to this runtime project.
3. Inherit from `ReGoap.Godot` base classes in your gameplay scripts.

This keeps one canonical runtime implementation and avoids duplicate script/type issues.

## Important project hygiene

- Do not duplicate generic `ReGoap*.cs` runtime files inside your game root.
- Keep generic runtime files in `ReGoap/Godot/`.
- Keep your game-specific actions/goals/memory/sensors in your game folder.

## Debugger

- Toggle: `F3`
- Switch agent: `<` and `>`
- Lock current agent: `Lock Agent`
- Pause live snapshots: `Pause`
- Export current snapshot: `Export JSON`

Export JSON behavior:

- writes snapshot file to `user://`
- attempts to open that folder in the OS file browser
- shows a dialog with saved file path and folder path

## Goal selection modes (global feature)

Planner supports deterministic and weighted-random goal selection via `ReGoapPlannerSettings`.

- deterministic (default): highest priority first
- weighted-random: priority-biased random choice

Relevant settings:

- `UseWeightedRandomGoalSelection`
- `WeightedRandomGoalPriorityPower`
- `WeightedRandomMinimumWeight`

## Comparator conditions (global feature)

`ReGoapCondition` supports:

- `Equal(value)`
- `NotEqual(value)`
- `GreaterOrEqual(value)`
- `LessOrEqual(value)`

These work in goals, preconditions and effects.

## Test suite (GdUnit4 + C#)

Godot tests are set up with GdUnit4 as a git submodule.

Submodule path:

- `ReGoap/Godot/FSMExample/addons/gdUnit4`

### Clone/update with submodules

Fresh clone:

```bash
git clone --recurse-submodules https://github.com/luxkun/ReGoap.git
```

Existing clone:

```bash
git submodule update --init --recursive
```

### C# test dependencies

FSMExample project includes GdUnit4 C# packages in:

- `ReGoap/Godot/FSMExample/regoap_godot_fsm_example.csproj`

### Run tests (required after every change)

After **any code change/new feature**, run both commands below in `ReGoap/Godot/FSMExample` and update/add tests to match behavior changes.

```bash
arch -arm64 env GODOT_BIN="/Applications/Godot_mono.app/Contents/MacOS/Godot" DOTNET_ROLL_FORWARD=Major dotnet test regoap_godot_fsm_example.csproj --settings gdunit.runsettings
```

```bash
arch -arm64 env GODOT_BIN="/Applications/Godot_mono.app/Contents/MacOS/Godot" DOTNET_ROLL_FORWARD=Major dotnet test regoap_godot_fsm_example.csproj --settings gdunit.runsettings --filter "FullyQualifiedName~WeightedRandomGoalSelectionTests.DeterministicSeedMatchesKnownFirstPicksSequence"
```

## Current Godot C# tests

- `ReGoap/Godot/FSMExample/Tests/ChestNodeTests.cs`
- `ReGoap/Godot/FSMExample/Tests/WorldResourceSensorTests.cs`
- `ReGoap/Godot/FSMExample/Tests/SwordsmithGoalTests.cs`

## AI agent instructions

If an AI coding agent is editing this subtree, read:

- `ReGoap/Godot/AGENT.md`
