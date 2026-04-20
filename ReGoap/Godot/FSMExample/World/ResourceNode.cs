using Godot;

namespace ReGoap.Godot.FSMExample.World
{
    public partial class ResourceNode : Node2D
    {
        [Export] public string ResourceName = "Tree";
        [Export] public int MaxCharges = 3;
        [Export] public float RegenSeconds = 4.0f;
        [Export] public Color Tint = new Color(0.25f, 0.55f, 0.25f);

        public int Charges { get; private set; }

        private bool regenRunning;
        private Label label;

        public override void _Ready()
        {
            Charges = MaxCharges;

            var body = new Polygon2D
            {
                Color = Tint,
                Polygon = new[] { new Vector2(-50, 40), new Vector2(50, 40), new Vector2(35, -35), new Vector2(-35, -35) }
            };
            AddChild(body);

            label = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Position = new Vector2(-90, 45),
                Size = new Vector2(180, 30)
            };
            AddChild(label);

            UpdateLabel();
        }

        public bool TryHarvest()
        {
            if (Charges <= 0)
                return false;

            Charges--;
            UpdateLabel();

            if (!regenRunning)
                _ = RegenLoop();

            return true;
        }

        private async System.Threading.Tasks.Task RegenLoop()
        {
            regenRunning = true;
            while (Charges < MaxCharges)
            {
                var timer = GetTree().CreateTimer(RegenSeconds);
                await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);
                Charges++;
                UpdateLabel();
            }
            regenRunning = false;
        }

        private void UpdateLabel()
        {
            if (label == null)
                return;

            label.Text = ResourceName + " " + Charges + "/" + MaxCharges;
        }
    }
}
