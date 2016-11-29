public class ReGoapTestAction : GoapAction
{
    public void Init()
    {
        Awake();
    }

    public void SetEffects(ReGoapState effects)
    {
        this.effects = effects;
    }

    public void SetPreconditions(ReGoapState preconditions)
    {
        this.preconditions = preconditions;
    }
}