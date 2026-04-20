namespace ReGoap.Core
{
    /// <summary>
    /// Represents one action step in a calculated plan,
    /// including planner-produced settings for that step.
    /// </summary>
    public class ReGoapActionState<T, W>
    {
        public IReGoapAction<T, W> Action;
        public ReGoapState<T, W> Settings;

        /// <summary>
        /// Creates an action-state pair for plan queues.
        /// </summary>
        public ReGoapActionState(IReGoapAction<T, W> action, ReGoapState<T, W> settings)
        {
            Action = action;
            Settings = settings;
        }
    }
}
