using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.VR;

// you could also create a generic ExternalGoToAction : GenericGoToAction
//  which let you add effects / preconditions from some source (Unity, external file, etc.)
//  and then add multiple ExternalGoToAction to your agent's gameobject's behaviours
// you can use this without any helper class by having the actions that need to move to a position
//  or transform to have a precondition isAtPosition or isAtTransform
[RequireComponent(typeof(SmsGoTo))]
public class GenericGoToAction : GoapAction
{
    // sometimes a Transform is better (moving target), sometimes you do not have one (last target position)
    protected Transform objectiveTransform;
    protected Vector3 objectivePosition;

    protected SmsGoTo smsGoto;

    protected override void Awake()
    {
        base.Awake();

        effects.Set("isAtPosition", ReGoapState.WildCard);
        effects.Set("isAtTransform", ReGoapState.WildCard);
        smsGoto = GetComponent<SmsGoTo>();
    }

    public override void Run(IReGoapAction previous, IReGoapAction next, ReGoapState goalState, Action<IReGoapAction> done, Action<IReGoapAction> fail)
    {
        base.Run(previous, next, goalState, done, fail);

        GetObjective(next);
        if (objectiveTransform != null)
            smsGoto.GoTo(objectiveTransform, OnDoneMovement, OnFailureMovement);
        else if (objectivePosition != default(Vector3))
            smsGoto.GoTo(objectivePosition, OnDoneMovement, OnFailureMovement);
        else
            failCallback(this);
    }

    // generic behaviour, get from the next action's generic values: 'objective' or 'objectiveTransform'
    // most of goto actions will override this function and set objective themselves
    protected virtual void GetObjective()
    {
        GetObjective(nextAction);
    }

    protected virtual void GetObjective(IReGoapAction next)
    {
        var nextPreconditions = next.GetPreconditions(null, previousAction);
        objectivePosition = nextPreconditions.Get<Vector3>("isAtPosition");
        objectiveTransform = nextPreconditions.Get<Transform>("isAtTransform");
    }

    public override ReGoapState GetEffects(ReGoapState goalState, IReGoapAction next = null)
    {
        if (next != null)
        {
            GetObjective(next);
            effects.Set("isAtPosition", objectivePosition);
            effects.Set("isAtTransform", objectiveTransform);
        }
        else
        {
            effects.Set("isAtPosition", ReGoapState.WildCard);
            effects.Set("isAtTransform", ReGoapState.WildCard);
        }
        return base.GetEffects(goalState, next);
    }

    public override float GetCost(ReGoapState goalState, IReGoapAction next = null)
    {
        var distance = 0;
        if (next != null)
        {
            GetObjective(next);
            if (objectivePosition != default(Vector3))
            {
                distance += Mathf.RoundToInt(Cost * (transform.position - objectivePosition).sqrMagnitude);
            }
            else if (objectiveTransform != null)
            {
                distance += Mathf.RoundToInt(Cost * (transform.position - objectiveTransform.transform.position).sqrMagnitude);
            }
        }
        return base.GetCost(goalState, next) + distance;
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
