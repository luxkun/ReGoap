using System;
using UnityEngine;
using System.Collections;

public class GoToTreeAction : GenericGoToAction
{
    protected override void Awake()
    {
        base.Awake();
        preconditions.Set("seeTree", true);
        effects.Set("isAt", "tree");
    }

    protected override void GetObjective()
    {
        var nearestTree = agent.GetMemory().GetWorldState().Get<IResource>("nearestTree");
        if (nearestTree == null)
            failCallback(this);
        else
            objectiveTransform = agent.GetMemory().GetWorldState().Get<IResource>("nearestTree").GetTransform();
    }
}
