using ReGoap.Core;

namespace ReGoap.Unity.Test
{
    public class ReGoapTestAction : ReGoapAction<string, object>
    {
        public void Init()
        {
            Awake();
        }

        public void SetEffects(ReGoapState<string, object> effects)
        {
            this.effects = effects;
        }

        public void SetPreconditions(ReGoapState<string, object> preconditions)
        {
            this.preconditions = preconditions;
        }
    }
}