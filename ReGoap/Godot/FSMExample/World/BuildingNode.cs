using Godot;

namespace ReGoap.Godot.FSMExample.World
{
    public partial class BuildingNode : Node2D
    {
        [Export] public string BuildingName = "Building";
        [Export] public Color Tint = new Color(0.35f, 0.35f, 0.42f);

        public override void _Ready()
        {
            var body = new Polygon2D
            {
                Color = Tint,
                Polygon = new[] { new Vector2(-70, 55), new Vector2(70, 55), new Vector2(70, -55), new Vector2(-70, -55) }
            };
            AddChild(body);

            var label = new Label
            {
                Text = BuildingName,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Position = new Vector2(-90, -15),
                Size = new Vector2(180, 30)
            };
            AddChild(label);
        }
    }
}
