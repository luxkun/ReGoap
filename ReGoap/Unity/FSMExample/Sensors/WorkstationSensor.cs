using System.Collections.Generic;
using ReGoap.Unity.FSMExample.OtherScripts;
using UnityEngine;

// using a sensor for this makes everything more dynamic, if the agent is pushed/moved/teleported he
//  will be able to understand dyamically if he's near the wanted objective
//  otherwise you can set such variables directly in the relative GoapAction (faster/simplier but less flexible)
namespace ReGoap.Unity.FSMExample.Sensors
{
    public class WorkstationSensor : ReGoapSensor<string, object>
    {
        private Dictionary<Workstation, Vector3> workstations;
        
        void Start()
        {
            workstations = new Dictionary<Workstation, Vector3>(WorkstationsManager.Instance.Workstations.Length);
            foreach (var workstation in WorkstationsManager.Instance.Workstations)
            {
                workstations[workstation] = workstation.transform.position; // workstations are static
            }

            var worldState = memory.GetWorldState();
            worldState.Set("workstations", workstations);
        }
    }
}
