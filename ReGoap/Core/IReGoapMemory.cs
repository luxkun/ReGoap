namespace ReGoap.Core
{
    /// <summary>
    /// Contract implemented by GOAP memory providers.
    /// </summary>
    public interface IReGoapMemory<T, W>
    {
        /// <summary>
        /// Returns mutable world state used by planner and sensors.
        /// </summary>
        ReGoapState<T, W> GetWorldState();
    }
}
