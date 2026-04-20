using System.Collections.Generic;
using ReGoap.Core;

namespace ReGoap.Godot
{
    public partial class ReGoapMemoryAdvanced<T, W> : ReGoapMemory<T, W>
    {
        private IReGoapSensor<T, W>[] sensors;

        public float SensorsUpdateDelay = 0.3f;
        private float sensorsUpdateCooldown;

        public override void _Ready()
        {
            base._Ready();
            sensors = new List<IReGoapSensor<T, W>>(GetSensorsInSubtree()).ToArray();
            foreach (var sensor in sensors)
            {
                sensor.Init(this);
            }
        }

        public override void _Process(double delta)
        {
            if (GetTime() > sensorsUpdateCooldown)
            {
                sensorsUpdateCooldown = GetTime() + SensorsUpdateDelay;
                foreach (var sensor in sensors)
                {
                    sensor.UpdateSensor();
                }
            }
        }

        protected virtual IEnumerable<IReGoapSensor<T, W>> GetSensorsInSubtree()
        {
            var queue = new Queue<global::Godot.Node>();
            queue.Enqueue(this);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                var sensor = node as IReGoapSensor<T, W>;
                if (sensor != null)
                    yield return sensor;

                foreach (var childObj in node.GetChildren())
                {
                    var child = childObj as global::Godot.Node;
                    if (child != null)
                        queue.Enqueue(child);
                }
            }
        }

        protected virtual float GetTime()
        {
            return (float)(System.DateTime.UtcNow.Ticks / (double)System.TimeSpan.TicksPerSecond);
        }
    }
}
