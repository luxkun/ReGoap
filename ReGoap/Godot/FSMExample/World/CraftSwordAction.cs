using ReGoap.Core;

namespace ReGoap.Godot.FSMExample.World
{
    public partial class CraftSwordAction : WorkerActionBase
    {
        public override void _Ready()
        {
            base._Ready();
            Name = "Craft Sword";
            Cost = 1.7f;
        }

        public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData)
        {
            preconditions.Set("chestWoodCount", ReGoapCondition.GreaterOrEqual(1));
            preconditions.Set("chestIngotCount", ReGoapCondition.GreaterOrEqual(1));
            return preconditions;
        }

        public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData)
        {
            var currentWoodObj = stackData.currentState.Get("chestWoodCount");
            var currentWood = currentWoodObj is int wood ? wood : 0;
            var currentIngotObj = stackData.currentState.Get("chestIngotCount");
            var currentIngot = currentIngotObj is int ingot ? ingot : 0;
            var currentSwordObj = stackData.currentState.Get("chestSwordCount");
            var currentSword = currentSwordObj is int sword ? sword : 0;

            effects.Set("chestWoodCount", currentWood > 0 ? currentWood - 1 : 0);
            effects.Set("chestIngotCount", currentIngot > 0 ? currentIngot - 1 : 0);
            effects.Set("chestSwordCount", currentSword + 1);
            return effects;
        }

        public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData)
        {
            var bindings = GetBindings();
            return bindings != null && bindings.World != null && bindings.World.Blacksmith != null && bindings.World.Chest != null;
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

            if (!bindings.World.Chest.ConsumeWood(1) || !bindings.World.Chest.ConsumeIronIngot(1))
            {
                SafeFail();
                return;
            }
            bindings.Pawn.SetCarrying(true);

            await bindings.Pawn.MoveToGlobal(bindings.World.Blacksmith.GlobalPosition + new global::Godot.Vector2(0, 110));
            if (FailIfInvalid(bindings))
                return;
            if (IsRunningStalePlan())
            {
                bindings.World.Chest.AddWood(1);
                bindings.World.Chest.AddIronIngot(1);
                SafeFail();
                return;
            }

            var timer = GetTree().CreateTimer(0.5);
            await ToSignal(timer, global::Godot.SceneTreeTimer.SignalName.Timeout);
            if (FailIfInvalid(bindings))
                return;
            if (IsRunningStalePlan())
            {
                bindings.World.Chest.AddWood(1);
                bindings.World.Chest.AddIronIngot(1);
                SafeFail();
                return;
            }

            await bindings.Pawn.MoveToGlobal(bindings.World.Chest.GlobalPosition + new global::Godot.Vector2(0, 110));
            if (FailIfInvalid(bindings))
                return;
            if (IsRunningStalePlan())
            {
                bindings.World.Chest.AddWood(1);
                bindings.World.Chest.AddIronIngot(1);
                SafeFail();
                return;
            }

            bindings.World.Chest.AddSwords(1);
            bindings.Pawn.SetCarrying(false);

            var ws = GetWorldState();
            ws.Set("chestWoodCount", bindings.World.Chest.Wood);
            ws.Set("chestIngotCount", bindings.World.Chest.IronIngot);
            ws.Set("chestSwordCount", bindings.World.Chest.Swords);

            SafeDone();
        }
    }
}
