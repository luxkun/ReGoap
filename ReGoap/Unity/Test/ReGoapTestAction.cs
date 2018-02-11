using ReGoap.Core;

using System.Collections.Generic;

namespace ReGoap.Unity.Test
{
    public class ReGoapTestAction : ReGoapAction<string, object>
    {
        public delegate void PreconditionsGetter(ref ReGoapState<string, object> preconditions, GoapActionStackData<string, object> stackData);
        public delegate void EffectsGetter(ref ReGoapState<string, object> effects, GoapActionStackData<string, object> stackData);
        public delegate void CostGetter(ref float cost, GoapActionStackData<string, object> stackData);
        public delegate List<ReGoapState<string, object>> SettingsGetter(GoapActionStackData<string, object> stackData);
        public PreconditionsGetter CustomPreconditionsGetter;
        public EffectsGetter CustomEffectsGetter;
        public CostGetter CustomCostGetter;
        public SettingsGetter CustomSettingsGetter;

        public void Init()
        {
            Awake();
        }

        public virtual void SetEffects(ReGoapState<string, object> effects)
        {
            this.effects = effects;
        }

        public virtual void SetPreconditions(ReGoapState<string, object> preconditions)
        {
            this.preconditions = preconditions;
        }

        public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData)
        {
            if (CustomSettingsGetter != null)
                return CustomSettingsGetter(stackData);
            return new List<ReGoapState<string, object>> { settings };
        }

        public override ReGoapState<string, object> GetPreconditions(GoapActionStackData<string, object> stackData)
        {
            if (CustomPreconditionsGetter != null)
                CustomPreconditionsGetter(ref preconditions, stackData);
            return preconditions;
        }

        public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData)
        {
            if (CustomEffectsGetter != null)
                CustomEffectsGetter(ref effects, stackData);
            return effects;
        }

        public override float GetCost(GoapActionStackData<string, object> stackData)
        {
            if (CustomCostGetter != null)
                CustomCostGetter(ref Cost, stackData);
            return Cost;
        }
    }
}