using ReGoap.Core;

namespace ReGoap.Godot.FSMExample.World
{
    public partial class SmeltIngotAction : WorkerActionBase
    {
        public override void _Ready()
        {
            base._Ready();
            Name = "Smelt Ingot";
            Cost = 1.4f;
        }

        public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData)
        {
            preconditions.Set("chestOreCount", ReGoapCondition.GreaterOrEqual(1));
            return preconditions;
        }

        public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData)
        {
            var currentOreObj = stackData.currentState.Get("chestOreCount");
            var currentOre = currentOreObj is int ore ? ore : 0;
            var currentIngotObj = stackData.currentState.Get("chestIngotCount");
            var currentIngot = currentIngotObj is int ingot ? ingot : 0;

            effects.Set("chestOreCount", currentOre > 0 ? currentOre - 1 : 0);
            effects.Set("chestIngotCount", currentIngot + 1);
            return effects;
        }

        public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData)
        {
            var bindings = GetBindings();
            return bindings != null && bindings.World != null && bindings.World.Smelter != null && bindings.World.Chest != null;
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

            if (!bindings.World.Chest.ConsumeIronOre(1))
            {
                SafeFail();
                return;
            }
            bindings.Pawn.SetCarrying(true);

            await bindings.Pawn.MoveToGlobal(bindings.World.Smelter.GlobalPosition + new global::Godot.Vector2(0, 110));
            if (FailIfInvalid(bindings))
                return;
            if (IsRunningStalePlan())
            {
                bindings.World.Chest.AddIronOre(1);
                SafeFail();
                return;
            }

            var timer = GetTree().CreateTimer(0.5);
            await ToSignal(timer, global::Godot.SceneTreeTimer.SignalName.Timeout);
            if (FailIfInvalid(bindings))
                return;
            if (IsRunningStalePlan())
            {
                bindings.World.Chest.AddIronOre(1);
                SafeFail();
                return;
            }

            await bindings.Pawn.MoveToGlobal(bindings.World.Chest.GlobalPosition + new global::Godot.Vector2(0, 110));
            if (FailIfInvalid(bindings))
                return;
            if (IsRunningStalePlan())
            {
                bindings.World.Chest.AddIronOre(1);
                SafeFail();
                return;
            }

            bindings.World.Chest.AddIronIngot(1);
            bindings.Pawn.SetCarrying(false);

            var ws = GetWorldState();
            ws.Set("chestOreCount", bindings.World.Chest.IronOre);
            ws.Set("chestIngotCount", bindings.World.Chest.IronIngot);
            SafeDone();
        }
    }
}
