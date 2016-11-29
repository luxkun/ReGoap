using UnityEngine;
using System.Collections;

public class CollectResourceGoal : GoapGoal
{
    public string resourceName;

    protected override void Awake()
    {
        base.Awake();
        goal.Set("collected" + resourceName, true);
    }

    public override string ToString()
    {
        return string.Format("GoapGoal('{0}', '{1}')", Name, resourceName);
    }
}
