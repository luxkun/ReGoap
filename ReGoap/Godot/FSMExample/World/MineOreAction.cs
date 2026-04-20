using ReGoap.Core;

namespace ReGoap.Godot.FSMExample.World
{
    public partial class MineOreAction : WorkerActionBase
    {
        public override void _Ready()
        {
            base._Ready();
            Name = "Mine Ore";
            Cost = 1.1f;
            preconditions.Set("hasOre", false);
            effects.Set("hasOre", true);
        }

        public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData)
        {
            var bindings = GetBindings();
            return bindings != null && bindings.World.FindBestMine().Charges > 0;
        }

        public override async void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next,
            ReGoapState<string, object> actionSettings, ReGoapState<string, object> goalState,
            System.Action<IReGoapAction<string, object>> done, System.Action<IReGoapAction<string, object>> fail)
        {
            base.Run(previous, next, actionSettings, goalState, done, fail);

            var bindings = GetBindings();
            if (FailIfInvalid(bindings))
            {
                return;
            }
            if (IsRunningStalePlan())
            {
                SafeFail();
                return;
            }

            var mine = bindings.World.FindBestMine();
            if (mine == null || mine.Charges <= 0)
            {
                SafeFail();
                return;
            }

            await bindings.Pawn.MoveToGlobal(mine.GlobalPosition + new global::Godot.Vector2(0, 80));
            if (FailIfInvalid(bindings))
                return;
            if (IsRunningStalePlan())
            {
                SafeFail();
                return;
            }
            bindings.Pawn.SetCarrying(true);

            if (!mine.TryHarvest())
            {
                bindings.Pawn.SetCarrying(false);
                SafeFail();
                return;
            }

            GetWorldState().Set("hasOre", true);
            SafeDone();
        }
    }
}
