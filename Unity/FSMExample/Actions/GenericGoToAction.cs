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

        SetDefaultEffects();
        smsGoto = GetComponent<SmsGoTo>();
    }

    protected virtual void SetDefaultEffects()
    {
        effects.Set("isAtPosition", default(Vector3));
        effects.Set<Transform>("isAtTransform", null);
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
        // do not use transforms if you plan to use multi-thread!
        objectiveTransform = nextPreconditions.Get<Transform>("isAtTransform");
    }

    // alternative way to get objective position
    protected virtual void GetObjective(ReGoapState goalState)
    {
        objectivePosition = goalState.Get<Vector3>("isAtPosition");
        // do not use transforms if you plan to use multi-thread!
        objectiveTransform = goalState.Get<Transform>("isAtTransform");
    }

    protected virtual Vector3 GetCurrentPositionFromMemory()
    {
        return agent.GetMemory().GetWorldState().Get<Vector3>("isAtPosition");
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
            SetDefaultEffects();
        }
        return base.GetEffects(goalState, next);
    }

    public override float GetCost(ReGoapState goalState, IReGoapAction next = null)
    {
        var distance = 0f;
        if (next != null)
        {
            var currentPosition = GetCurrentPositionFromMemory();
            GetObjective(goalState); // seems better to get this information from the goal
            if (objectivePosition != default(Vector3))
            {
                distance += Cost * (currentPosition - objectivePosition).sqrMagnitude;
            }
            else if (objectiveTransform != null)
            {
                distance += Cost * (currentPosition - objectiveTransform.transform.position).sqrMagnitude;
            }
            Debug.Log("Current goto cost: " + distance);
        }
        return base.GetCost(goalState, next) + Cost + distance;
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
