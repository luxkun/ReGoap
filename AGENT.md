# ReGoap Agent Notes

This file gives AI coding agents repo-wide guidance.

## Scope

- Applies to the whole repository.
- More specific `AGENT.md` files in subdirectories override this file for their subtree.

## Repository Structure

- Core planner/runtime: `ReGoap/Core`, `ReGoap/Planner`, `ReGoap/Utilities`
- Godot adapter/runtime: `ReGoap/Godot`
- Unity adapter/runtime: `ReGoap/Unity`

## Documentation Split

- Root `README.md` explains GOAP concepts and shared/core architecture.
- Engine-specific implementation details belong in:
  - `ReGoap/Godot/README.md`
  - `ReGoap/Unity/README.md`

## Conventions

- Keep shared runtime code generic and engine-agnostic where possible.
- Keep Godot- and Unity-specific behavior in their respective adapter folders.
- Prefer updating shared base classes over duplicating engine/example-specific copies.
- Add or update tests when behavior changes.

## Safety

- Do not add duplicate source files for existing shared classes.
- For Godot FSM example, shared classes are included from parent folders by csproj includes.
- If script loading fails, fix paths/imports instead of adding duplicate source files.

## Subtree Overrides

- `ReGoap/Godot/AGENT.md` contains Godot-only workflow and runtime notes.
- If an instruction here conflicts with subtree guidance, follow the subtree file.
