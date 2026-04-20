using System;
using System.Threading.Tasks;
using Godot;

namespace ReGoap.Godot.FSMExample.World
{
    public partial class BuilderActor : Node2D
    {
        [Export] public Vector2 HomePosition = new Vector2(260, 500);
        [Export] public Vector2 ResourcePosition = new Vector2(940, 500);
        [Export] public float MoveSpeed = 220.0f;

        private bool carrying;
        private Polygon2D carryMarker;

        public override void _Ready()
        {
            Position = HomePosition;
            carryMarker = GetNodeOrNull<Polygon2D>("Carry");
            UpdateCarryVisual();
        }

        public async Task PerformGatherCycle()
        {
            await MoveTo(ResourcePosition);
            carrying = true;
            UpdateCarryVisual();

            await Wait(0.2f);

            await MoveTo(HomePosition);
            carrying = false;
            UpdateCarryVisual();
        }

        private async Task MoveTo(Vector2 target)
        {
            var duration = Math.Max(0.05, Position.DistanceTo(target) / MoveSpeed);
            var tween = CreateTween();
            tween.SetTrans(Tween.TransitionType.Sine);
            tween.SetEase(Tween.EaseType.InOut);
            tween.TweenProperty(this, "position", target, duration);
            await ToSignal(tween, Tween.SignalName.Finished);
        }

        private async Task Wait(float seconds)
        {
            var timer = GetTree().CreateTimer(seconds);
            await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);
        }

        private void UpdateCarryVisual()
        {
            if (carryMarker != null)
                carryMarker.Visible = carrying;
        }
    }
}
