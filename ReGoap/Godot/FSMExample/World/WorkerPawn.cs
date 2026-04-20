using System;
using Godot;

namespace ReGoap.Godot.FSMExample.World
{
    public partial class WorkerPawn : Node2D
    {
        [Export] public Vector2 HomePosition = new Vector2(180, 620);
        [Export] public float MoveSpeed = 250.0f;
        [Export] public Color BodyColor = new Color(0.92f, 0.85f, 0.35f);

        private Polygon2D carryMarker;

        public override void _Ready()
        {
            Position = HomePosition;

            var body = new Polygon2D
            {
                Color = BodyColor,
                Polygon = new[] { new Vector2(-16, 20), new Vector2(16, 20), new Vector2(16, -20), new Vector2(-16, -20) }
            };
            AddChild(body);

            carryMarker = new Polygon2D
            {
                Visible = false,
                Position = new Vector2(0, -28),
                Color = new Color(0.75f, 0.52f, 0.25f),
                Polygon = new[] { new Vector2(-8, 8), new Vector2(8, 8), new Vector2(8, -8), new Vector2(-8, -8) }
            };
            AddChild(carryMarker);
        }

        public async System.Threading.Tasks.Task MoveToGlobal(Vector2 target)
        {
            var duration = Math.Max(0.05, GlobalPosition.DistanceTo(target) / MoveSpeed);
            var tween = CreateTween();
            tween.SetTrans(Tween.TransitionType.Sine);
            tween.SetEase(Tween.EaseType.InOut);
            tween.TweenProperty(this, "global_position", target, duration);
            await ToSignal(tween, Tween.SignalName.Finished);
        }

        public void SetCarrying(bool carrying)
        {
            if (carryMarker != null)
                carryMarker.Visible = carrying;
        }
    }
}
