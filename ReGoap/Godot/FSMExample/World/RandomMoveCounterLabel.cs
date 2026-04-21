using System.Text;
using Godot;

namespace ReGoap.Godot.FSMExample.World
{
    /// <summary>
    /// Lightweight HUD label showing per-worker and total random-move completions.
    /// </summary>
    public partial class RandomMoveCounterLabel : Label
    {
        [Export] public NodePath WorkersPath = new NodePath("../Workers");

        public override void _Ready()
        {
            Position = new Vector2(16, 16);
            Modulate = new Color(0.94f, 0.95f, 0.96f, 0.95f);
            UpdateText();
        }

        public override void _Process(double delta)
        {
            UpdateText();
        }

        private void UpdateText()
        {
            var workersRoot = GetNodeOrNull<Node>(WorkersPath);
            if (workersRoot == null)
            {
                Text = "Random Move Count: workers missing";
                return;
            }

            var total = 0;
            var workers = new System.Collections.Generic.List<(string name, int count)>();

            foreach (var childObj in workersRoot.GetChildren())
            {
                if (childObj is not WorkerAgent worker)
                    continue;

                var workerName = worker.Name;
                var memory = worker.GetNodeOrNull<WorkerMemory>("Memory");
                var count = 0;
                if (memory != null)
                {
                    var value = memory.GetWorldState().Get("randomMoveCount");
                    if (value is int intValue)
                        count = intValue;
                }

                total += count;
                workers.Add((workerName, count));
            }

            var sb = new StringBuilder();
            sb.Append("Random Move Count  Total=");
            sb.Append(total);
            foreach (var worker in workers)
            {
                sb.Append('\n');
                sb.Append(worker.name);
                sb.Append(": ");
                sb.Append(worker.count);
            }

            Text = sb.ToString();
        }
    }
}
