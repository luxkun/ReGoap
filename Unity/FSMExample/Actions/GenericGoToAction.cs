using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// this is just an helper class, you could also create a generic ExternalGoToAction : GenericGoToAction
//  which let you add effects / preconditions from some source (Unity, external file, etc.)
//  and then add multiple ExternalGoToAction to your agent's gameobject's behaviours
// abstract because added like this won't work (no effect/precondition): check GoToWoodCollectorAction
[RequireComponent(typeof(SmsGoTo))]
public abstract class GenericGoToAction : GoapAction
{
    // sometimes a Transform is better (moving target), sometimes you do not have one (last target position)
    protected Transform objectiveTransform;
    protected Vector3 objectivePosition;

    protected SmsGoTo smsGoto;

    protected override void Awake()
    {
        base.Awake();

        smsGoto = GetComponent<SmsGoTo>();
    }

    public override void Run(IReGoapAction previous, IReGoapAction next, ReGoapState goalState, Action<IReGoapAction> done, Action<IReGoapAction> fail)
    {
        base.Run(previous, next, goalState, done, fail);

        GetObjective();
        if (objectiveTransform != null)
            smsGoto.GoTo(objectiveTransform, OnDoneMovement, OnFailureMovement);
        else
            smsGoto.GoTo(objectivePosition, OnDoneMovement, OnFailureMovement);
    }

    // generic behaviour, get from the next action's generic values: 'objective' or 'objectiveTransform'
    // most of goto actions will override this function and set objective themselves
    protected virtual void GetObjective()
    {
        var objective = nextAction.GetGenericValues()["objective"];
        var objectiveTransform = nextAction.GetGenericValues()["objectiveTransform"];
        if (objectiveTransform != null)
        {
            this.objectiveTransform = (Transform)objectiveTransform;
        }
        else if (objective != null)
        {
            this.objectivePosition = (Vector3)objective;
        }
        else
        {
            throw new UnityException(string.Format("[{0}] Next action's does not have a generic value 'objective' (Vector3) or 'objectiveTransform' (Transform).", GetType().FullName));
        }
    }

    protected virtual void OnFailureMovement()
    {
        failCallback(this);
    }

    protected virtual void OnDoneMovement()
    {
        doneCallback(this);
    }
}
