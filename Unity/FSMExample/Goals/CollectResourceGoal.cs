using UnityEngine;
using System.Collections;

public class CollectResourceGoal : GoapGoal
{
    public string ResourceName;

    protected override void Awake()
    {
        base.Awake();
        goal.Set("collectedResource" + ResourceName, true);
    }

    public override string ToString()
    {
        return string.Format("GoapGoal('{0}', '{1}')", Name, ResourceName);
    }
}

