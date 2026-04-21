# ReGoap Godot Agent Notes

This file gives AI coding agents guidance for the Godot subtree.

## Scope

- Applies to `ReGoap/Godot/**`.
- Overrides root `AGENT.md` for this subtree where instructions differ.

## Purpose

`ReGoapDebugger` is a runtime debugging UI for Godot that mirrors Unity debugger intent:

- inspect goals
- inspect possible actions and failed preconditions
- inspect current plan and active action
- inspect world state
- view a graph-style layout
- freeze and browse snapshots
- export snapshot data as JSON

## Key Files

- Runtime debugger: `ReGoap/Godot/ReGoapDebugger.cs`
- Launch context helper: `ReGoap/Godot/ReGoapLaunchContext.cs`
- Example debugger root: `ReGoap/Godot/FSMExample/Debug/DebuggerRoot.cs`
- Example scene with debugger attached: `ReGoap/Godot/FSMExample/Scenes/TestMap.tscn`

## Runtime Controls

- Toggle debugger visibility: `F3`
- Previous/next agent: `<` and `>` buttons
- Lock selected agent: `Lock Agent`
- Pause/resume live updates: `Pause`
- Filter text output: `Filter` field
- Select historical snapshot while paused: `Snapshot` dropdown
- Export current snapshot JSON: `Export JSON`

## Data Flow (High-Level)

1. Discover `IReGoapAgentHelper` nodes from current scene tree.
2. Build a typed snapshot via reflection (`BuildSnapshot` -> `BuildSnapshotTyped<T, W>`).
3. Populate:
   - text sections (goals, actions, plan, world state)
   - graph nodes/connections
4. Append snapshot to in-memory history.
5. Render selected snapshot in tabs.

## Extension Points

- Add new text sections:
  - update `Snapshot` model
  - fill in `BuildSnapshotTyped<T, W>`
  - render in `RenderCurrentSnapshot`
- Add new graph node categories:
  - add enum value to `GraphNodeKind`
  - map color in `ColorForKind`
  - emit nodes in snapshot builders
- Add richer JSON export:
  - update `ExportCurrentSnapshot`

## Validation Checklist

After debugger changes, run:

```bash
dotnet build ReGoap/Godot/FSMExample/regoap_godot_fsm_example.csproj
```

Then verify in Godot:

1. open `ReGoap/Godot/FSMExample/project.godot`
2. run `Scenes/TestMap.tscn`
3. press `F3` and validate both tabs (`Inspector`, `Graph`)
4. pause, switch snapshots, export JSON

## Test Policy

- Godot tests in this repository must be C# tests.
- Do not add new GDScript tests.
- Place Godot C# tests under `ReGoap/Godot/FSMExample/Tests`.
- Use GdUnit4 C# API attributes (`[TestSuite]`, `[TestCase]`) and assertions.

## Notes

- This debugger is runtime UI (not an editor plugin), so it also works in exported debug builds.
- Nullability warnings currently exist in the codebase; they are known and not specific to the debugger.

## Troubleshooting

- Do not create local symlinks/copies of shared `ReGoap*.cs` inside `ReGoap/Godot/FSMExample` root while `regoap_godot_fsm_example.csproj` includes parent files.
- Keep shared Godot runtime classes in `ReGoap/Godot/*.cs` and include them from csproj.
- If script loading fails at runtime, fix script paths/imports in scene/resources instead of adding duplicate source files.
