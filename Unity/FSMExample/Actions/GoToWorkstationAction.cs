using UnityEngine;
using System.Collections;

public class GoToWorkstationAction : GenericGoToAction
{
    protected override void Awake()
    {
        base.Awake();
        preconditions.Set("seeWorkstation", true);
        effects.Set("isAt", "workstation");
    }

    public override bool CheckProceduralCondition(IReGoapAgent goapAgent, ReGoapState goalState)
    {
        return base.CheckProceduralCondition(goapAgent, goalState) && WorkstationsManager.instance != null;
    }

    protected override void GetObjective()
    {
        objectiveTransform = WorkstationsManager.instance.GetWorkstation().transform;
    }
}
