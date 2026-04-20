using Godot;

namespace ReGoap.Godot.FSMExample.World
{
    public partial class ChestNode : Node2D
    {
        public int Wood { get; private set; }
        public int IronOre { get; private set; }
        public int IronIngot { get; private set; }
        public int Swords { get; private set; }

        private Label label;

        public override void _Ready()
        {
            var box = new Polygon2D
            {
                Color = new Color(0.42f, 0.29f, 0.15f),
                Polygon = new[] { new Vector2(-70, 40), new Vector2(70, 40), new Vector2(70, -40), new Vector2(-70, -40) }
            };
            AddChild(box);

            var lid = new Polygon2D
            {
                Color = new Color(0.59f, 0.41f, 0.20f),
                Polygon = new[] { new Vector2(-75, -30), new Vector2(75, -30), new Vector2(75, -50), new Vector2(-75, -50) }
            };
            AddChild(lid);

            label = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Position = new Vector2(-130, 60),
                Size = new Vector2(260, 110)
            };
            AddChild(label);

            UpdateLabel();
        }

        public void AddWood(int amount)
        {
            Wood += amount;
            UpdateLabel();
        }

        public bool ConsumeWood(int amount)
        {
            if (Wood < amount)
                return false;
            Wood -= amount;
            UpdateLabel();
            return true;
        }

        public void AddIronOre(int amount)
        {
            IronOre += amount;
            UpdateLabel();
        }

        public bool ConsumeIronOre(int amount)
        {
            if (IronOre < amount)
                return false;
            IronOre -= amount;
            UpdateLabel();
            return true;
        }

        public void AddIronIngot(int amount)
        {
            IronIngot += amount;
            UpdateLabel();
        }

        public bool ConsumeIronIngot(int amount)
        {
            if (IronIngot < amount)
                return false;
            IronIngot -= amount;
            UpdateLabel();
            return true;
        }

        public void AddSwords(int amount)
        {
            Swords += amount;
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (label == null)
                return;

            label.Text = "Chest\n" +
                         "Wood: " + Wood + "\n" +
                         "Iron Ore: " + IronOre + "\n" +
                         "Ingots: " + IronIngot + "\n" +
                         "Swords: " + Swords;
        }
    }
}
