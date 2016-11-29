using UnityEngine;
using System.Collections;


public class GoToBankAction : GenericGoToAction
{
    protected override void Awake()
    {
        base.Awake();
        preconditions.Set("seeBank", true);
        effects.Set("isAt", "bank");
    }

    public override bool CheckProceduralCondition(IReGoapAgent goapAgent, ReGoapState goalState)
    {
        return base.CheckProceduralCondition(goapAgent, goalState) && BankManager.instance != null &&
               BankManager.instance.GetBanksCount() > 0;
    }

    protected override void GetObjective()
    {
        objectiveTransform = BankManager.instance.GetBank().transform;
    }
}
