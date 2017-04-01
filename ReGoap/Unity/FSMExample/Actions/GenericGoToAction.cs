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
public class GenericGoToAction : ReGoapAction<string, object>
{
    // sometimes a Transform is better (moving target), sometimes you do not have one (last target position)
    //  but if you're using multi-thread approach you can't use a transform or any unity's API
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
    }

    // generic behaviour, get from the goal values: 'isAtPosition'

    protected virtual Vector3? GetCurrentPositionFromMemory()
    {
        return agent.GetMemory().GetWorldState().Get("isAtPosition") as Vector3?;
    }

    public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, IReGoapActionSettings<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
    {
        base.Run(previous, next, settings, goalState, done, fail);

        var localSettings = (GenericGoToSettings) settings;

        if (localSettings.ObjectivePosition.HasValue)
            smsGoto.GoTo(localSettings.ObjectivePosition, OnDoneMovement, OnFailureMovement);
        else
            failCallback(this);
    }

    public override ReGoapState<string, object> GetEffects(ReGoapState<string, object> goalState, IReGoapAction<string, object> next = null)
    {
        var goalWantedPosition = GetWantedPositionFromState(goalState);
        if (goalWantedPosition.HasValue)
        {
            effects.Set("isAtPosition", goalWantedPosition);
        }
        else
        {
            SetDefaultEffects();
        }
        return base.GetEffects(goalState, next);
    }

    Vector3? GetWantedPositionFromState(ReGoapState<string, object> state)
    {
        Vector3? result = null;
        if (state != null)
        {
            result = state.Get("isAtPosition") as Vector3?;
        }
        return result;
    }

    public override IReGoapActionSettings<string, object> GetSettings(IReGoapAgent<string, object> goapAgent, ReGoapState<string, object> goalState)
    {
        settings = new GenericGoToSettings
        {
            ObjectivePosition = GetWantedPositionFromState(goalState)
        };
        return base.GetSettings(goapAgent, goalState);
    }

    // if you want to calculate costs use a non-dynamic/generic goto action
    public override float GetCost(ReGoapState<string, object> goalState, IReGoapAction<string, object> next = null)
    {
        return base.GetCost(goalState, next) + Cost;
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

public struct GenericGoToSettings : IReGoapActionSettings<string, object>
{
    public Vector3? ObjectivePosition;
}
