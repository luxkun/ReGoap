public class ReGoapActionState<T, W>
{
    public IReGoapAction<T, W> Action;
    public IReGoapActionSettings<T, W> Settings;

    public ReGoapActionState(IReGoapAction<T, W> action, IReGoapActionSettings<T, W> settings)
    {
        Action = action;
        Settings = settings;
    }
}