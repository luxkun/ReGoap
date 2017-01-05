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
//  or transform to have a precondition isAtPosition
[RequireComponent(typeof(SmsGoTo))]
public class GenericGoToAction : GoapAction
{
    // sometimes a Transform is better (moving target), sometimes you do not have one (last target position)
    //  but if you're using multi-thread approach you can't use a transform or any unity's API
    protected Vector3 objectivePosition;

    protected SmsGoTo smsGoto;

    protected override void Awake()
    {
        base.Awake();

        SetDefaultEffects();
        smsGoto = GetComponent<SmsGoTo>();
    }

    public override void Precalculations(IReGoapAgent goapAgent, ReGoapState goalState)
    {
        objectivePosition = goalState.Get<Vector3>("isAtPosition");
        settings = new GenericGoToSettings
        {
            ObjectivePosition = objectivePosition
        };
        base.Precalculations(goapAgent, goalState);
    }

    protected virtual void SetDefaultEffects()
    {
        effects.Set("isAtPosition", default(Vector3));
    }

    // generic behaviour, get from the goal values: 'isAtPosition'

    protected virtual Vector3 GetCurrentPositionFromMemory()
    {
        return agent.GetMemory().GetWorldState().Get<Vector3>("isAtPosition");
    }

    public override void Run(IReGoapAction previous, IReGoapAction next, IReGoapActionSettings settings, ReGoapState goalState, Action<IReGoapAction> done, Action<IReGoapAction> fail)
    {
        base.Run(previous, next, settings, goalState, done, fail);

        SetObjective(settings);
        if (objectivePosition != default(Vector3))
            smsGoto.GoTo(objectivePosition, OnDoneMovement, OnFailureMovement);
        else
            failCallback(this);
    }

    private void SetObjective(IReGoapActionSettings settings)
    {
        var thisSettings = (GenericGoToSettings) settings;
        objectivePosition = thisSettings.ObjectivePosition;
    }

    public override ReGoapState GetEffects(ReGoapState goalState, IReGoapAction next = null)
    {
        if (objectivePosition != default(Vector3))
        {
            effects.Set("isAtPosition", objectivePosition);
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
        //if (next != null)
        //{
        //    var currentPosition = GetCurrentPositionFromMemory();
        //    if (objectivePosition != default(Vector3))
        //    {
        //        distance += Cost * (currentPosition - objectivePosition).sqrMagnitude;
        //    }
        //}
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

internal class GenericGoToSettings : IReGoapActionSettings
{
    public Vector3 ObjectivePosition;
}
