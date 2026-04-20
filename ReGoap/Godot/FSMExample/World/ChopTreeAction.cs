using ReGoap.Core;

namespace ReGoap.Godot.FSMExample.World
{
    public partial class ChopTreeAction : WorkerActionBase
    {
        public override void _Ready()
        {
            base._Ready();
            Name = "Chop Tree";
            Cost = 1.0f;
            preconditions.Set("hasWood", false);
            effects.Set("hasWood", true);
        }

        public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData)
        {
            var bindings = GetBindings();
            return bindings != null && bindings.World.FindBestTree().Charges > 0;
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

            var tree = bindings.World.FindBestTree();
            if (tree == null || tree.Charges <= 0)
            {
                SafeFail();
                return;
            }

            await bindings.Pawn.MoveToGlobal(tree.GlobalPosition + new global::Godot.Vector2(0, 80));
            if (FailIfInvalid(bindings))
                return;
            if (IsRunningStalePlan())
            {
                SafeFail();
                return;
            }
            bindings.Pawn.SetCarrying(true);

            if (!tree.TryHarvest())
            {
                bindings.Pawn.SetCarrying(false);
                SafeFail();
                return;
            }

            GetWorldState().Set("hasWood", true);
            SafeDone();
        }
    }
}
