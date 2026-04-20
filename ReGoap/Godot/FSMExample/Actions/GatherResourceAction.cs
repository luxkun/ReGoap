using System;
using ReGoap.Godot.FSMExample.World;
using ReGoap.Core;
using ReGoap.Godot;

namespace ReGoap.Godot.FSMExample.Actions
{
    public partial class GatherResourceAction : ReGoapAction<string, object>
    {
        public string ResourceName = "Wood";

        public override void _Ready()
        {
            base._Ready();
            Name = "Gather " + ResourceName;
            effects.Set("collectedResource" + ResourceName, true);
        }

        public override void Run(
            IReGoapAction<string, object> previous,
            IReGoapAction<string, object> next,
            ReGoapState<string, object> actionSettings,
            ReGoapState<string, object> goalState,
            Action<IReGoapAction<string, object>> done,
            Action<IReGoapAction<string, object>> fail)
        {
            base.Run(previous, next, actionSettings, goalState, done, fail);

            var actor = GetNodeOrNull<BuilderActor>("../../BuilderActor");
            if (actor == null)
            {
                SafeFail();
                return;
            }

            _ = RunGather(actor);
        }

        private async System.Threading.Tasks.Task RunGather(BuilderActor actor)
        {
            try
            {
                await actor.PerformGatherCycle();

                if (!global::Godot.GodotObject.IsInstanceValid(this) || !IsInsideTree())
                    return;

                var worldState = agent.GetMemory().GetWorldState();
                worldState.Set("collectedResource" + ResourceName, true);
                SafeDone();
                _ = ResetGoalFlagLater(worldState);
            }
            catch
            {
                SafeFail();
            }
        }

        private async System.Threading.Tasks.Task ResetGoalFlagLater(ReGoapState<string, object> worldState)
        {
            var timer = GetTree().CreateTimer(0.8);
            await ToSignal(timer, global::Godot.SceneTreeTimer.SignalName.Timeout);
            if (!global::Godot.GodotObject.IsInstanceValid(this) || !IsInsideTree())
                return;
            worldState.Set("collectedResource" + ResourceName, false);
        }

        private void SafeDone()
        {
            doneCallback?.Invoke(this);
        }

        private void SafeFail()
        {
            failCallback?.Invoke(this);
        }
    }
}
