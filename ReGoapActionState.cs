public class ReGoapActionState
{
    public IReGoapAction Action;
    public IReGoapActionSettings Settings;

    public ReGoapActionState(IReGoapAction action, IReGoapActionSettings settings)
    {
        Action = action;
        Settings = settings;
    }
}