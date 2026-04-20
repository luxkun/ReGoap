using ReGoap.Core;

namespace ReGoap.Godot.FSMExample.World
{
    public partial class DepositOreAction : WorkerActionBase
    {
        public override void _Ready()
        {
            base._Ready();
            Name = "Deposit Ore";
            Cost = 0.55f;
            preconditions.Set("hasOre", true);
            effects.Set("hasOre", false);
        }

        public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData)
        {
            var currentOreObj = stackData.currentState.Get("chestOreCount");
            var currentOre = currentOreObj is int ore ? ore : 0;

            effects.Set("hasOre", false);
            effects.Set("chestOreCount", currentOre + 1);
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
            bindings.World.Chest.AddIronOre(1);
            bindings.Pawn.SetCarrying(false);

            var ws = GetWorldState();
            ws.Set("hasOre", false);
            ws.Set("chestOreCount", bindings.World.Chest.IronOre);
            SafeDone();
        }
    }
}
