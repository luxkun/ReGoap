using ReGoap.Core;

namespace ReGoap.Godot
{
    /// <summary>
    /// Base Godot sensor component that writes observations into memory.
    /// </summary>
    public partial class ReGoapSensor<T, W> : global::Godot.Node, IReGoapSensor<T, W>
    {
        protected IReGoapMemory<T, W> memory;

        /// <summary>
        /// Binds this sensor to a memory provider.
        /// </summary>
        public virtual void Init(IReGoapMemory<T, W> memory)
        {
            this.memory = memory;
        }

        /// <summary>
        /// Returns bound memory.
        /// </summary>
        public virtual IReGoapMemory<T, W> GetMemory()
        {
            return memory;
        }

        /// <summary>
        /// Sensor update hook.
        /// </summary>
        public virtual void UpdateSensor()
        {
        }
    }
}
