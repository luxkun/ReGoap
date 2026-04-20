namespace ReGoap.Core
{
    /// <summary>
    /// Optional contract for reusable memory updaters.
    /// </summary>
    public interface IReGoapSensor<T, W>
    {
        /// <summary>
        /// Attaches the sensor to a memory instance.
        /// </summary>
        void Init(IReGoapMemory<T, W> memory);

        /// <summary>
        /// Returns bound memory instance.
        /// </summary>
        IReGoapMemory<T, W> GetMemory();

        /// <summary>
        /// Executes sensor update and writes values to memory.
        /// </summary>
        void UpdateSensor();
    }
}
