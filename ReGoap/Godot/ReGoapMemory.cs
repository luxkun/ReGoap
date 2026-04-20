using ReGoap.Core;

namespace ReGoap.Godot
{
    /// <summary>
    /// Base Godot memory component holding shared world state.
    /// </summary>
    public partial class ReGoapMemory<T, W> : global::Godot.Node, IReGoapMemory<T, W>
    {
        protected ReGoapState<T, W> state;

        /// <summary>
        /// Allocates world state container.
        /// </summary>
        public override void _Ready()
        {
            state = ReGoapState<T, W>.Instantiate();
        }

        /// <summary>
        /// Recycles world state on node teardown.
        /// </summary>
        public override void _ExitTree()
        {
            if (state != null)
            {
                state.Recycle();
                state = null;
            }
        }

        /// <summary>
        /// Returns mutable world state map.
        /// </summary>
        public virtual ReGoapState<T, W> GetWorldState()
        {
            return state;
        }
    }
}
