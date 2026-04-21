# ReGoap Unity

## Quick Start
1. Clone this repository into your Unity project.
2. Create an Agent GameObject.
3. Add your custom classes derived from:
   - `ReGoapAgent<T, W>`
   - `ReGoapMemory<T, W>`
   - `ReGoapAction<T, W>`
   - `ReGoapGoal<T, W>`
4. Add one planner manager derived from `ReGoapPlannerManager<T, W>` in scene.
5. Play.

## Debugging
Unity debugger window is available through Unity editor menu:

- `Window -> ReGoap Debugger`

It shows runtime world state, goals, plan, possible actions and precondition failures.

## Examples
- Main Unity FSM example: `ReGoap/Unity/FSMExample`
- Runtime tests/helpers: `ReGoap/Unity/Test`
- Editor tests: `ReGoap/Unity/Editor/Test`

## Notes
- Keep the same generic pair (`T, W`) across agent/actions/goals/memory/sensors.
- Typical choice is `string, object`.
