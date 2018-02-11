namespace ReGoap.Core
{
    public class ReGoapActionState<T, W>
    {
        public IReGoapAction<T, W> Action;
        public ReGoapState<T, W> Settings;

        public ReGoapActionState(IReGoapAction<T, W> action, ReGoapState<T, W> settings)
        {
            Action = action;
            Settings = settings;
        }
    }
}