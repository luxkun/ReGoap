using ReGoap.Core;

namespace ReGoap.Godot.FSMExample.World
{
    public partial class DepositWoodAction : WorkerActionBase
    {
        public override void _Ready()
        {
            base._Ready();
            Name = "Deposit Wood";
            Cost = 0.5f;
            preconditions.Set("hasWood", true);
            effects.Set("hasWood", false);
        }

        public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData)
        {
            var currentWoodObj = stackData.currentState.Get("chestWoodCount");
            var currentWood = currentWoodObj is int wood ? wood : 0;

            effects.Set("hasWood", false);
            effects.Set("chestWoodCount", currentWood + 1);
            return effects;
        }

        public override async void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next,
            ReGoapState<string, object> actionSettings, ReGoapState<string, object> goalState,
            System.Action<IReGoapAction<string, object>> done, System.Action<IReGoapAction<string, object>> fail)
        {
            base.Run(previous, next, actionSettings, goalState, done, fail);
            var bindings = GetBindings();
            if (FailIfInvalid(bindings))
                return;
            if (IsRunningStalePlan())
            {
                SafeFail();
                return;
            }

            await bindings.Pawn.MoveToGlobal(bindings.World.Chest.GlobalPosition + new global::Godot.Vector2(0, 110));
            if (FailIfInvalid(bindings))
                return;
            if (IsRunningStalePlan())
            {
                SafeFail();
                return;
            }
            bindings.World.Chest.AddWood(1);
            bindings.Pawn.SetCarrying(false);

            var ws = GetWorldState();
            ws.Set("hasWood", false);
            ws.Set("chestWoodCount", bindings.World.Chest.Wood);
            SafeDone();
        }
    }
}
