using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// using a sensor for this makes everything more dynamic, if the agent is pushed/moved/teleported he
//  will be able to understand dyamically if he's near the wanted objective
//  otherwise you can set such variables directly in the relative GoapAction (faster/simplier but less flexible)
public class WorkstationSensor : GoapSensor
{
    private Dictionary<Workstation, Vector3> workstations;

    public float minPowDistanceToBeNear = 1f;

    void Start()
    {
        workstations = new Dictionary<Workstation, Vector3>(WorkstationsManager.instance.workstations.Length);
        foreach (var workstation in WorkstationsManager.instance.workstations)
        {
            workstations[workstation] = workstation.transform.position; // workstations are static
        }
    }

    public override void UpdateSensor()
    {
        var worldState = memory.GetWorldState();
        worldState.Set("seeWorkstation", WorkstationsManager.instance != null && WorkstationsManager.instance.workstations.Length > 0);

        var nearestStation = Utilities.GetNearest(transform.position, workstations);
        worldState.Set("nearestWorkstation", nearestStation);
        if (nearestStation != null &&
            (transform.position - nearestStation.transform.position).sqrMagnitude < minPowDistanceToBeNear)
        {
            worldState.Set("isAtTransform", nearestStation.transform);
        }
        else if (nearestStation != null && worldState.Get<Transform>("isAtTransform") == nearestStation.transform)
        {
            worldState.Set<Transform>("isAtTransform", null);
        }
    }
}
