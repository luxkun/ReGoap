using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class GoapAction : MonoBehaviour, IReGoapAction
{
    public string Name = "GoapAction";

    protected ReGoapState preconditions;
    protected ReGoapState effects;
    public int Cost = 1;

    protected Action<IReGoapAction> doneCallback;
    protected Action<IReGoapAction> failCallback;
    protected IReGoapAction previousAction;
    protected IReGoapAction nextAction;

    protected IReGoapAgent agent;
    protected Dictionary<string, object> genericValues;
    protected GoapActionSettings settings;
    protected bool interruptWhenPossible;

    #region UnityFunctions
    protected virtual void Awake()
    {
        enabled = false;

        effects = new ReGoapState();
        preconditions = new ReGoapState();

        genericValues = new Dictionary<string, object>();
        settings = new GoapActionSettings();
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void Update()
    {
        
    }

    protected virtual void FixedUpdate()
    {
    }
    #endregion

    public virtual bool IsActive()
    {
        return enabled;
    }

    public virtual void PostPlanCalculations(IReGoapAgent goapAgent)
    {
        agent = goapAgent;
    }

    public virtual bool IsInterruptable()
    {
        return true;
    }

    public virtual void AskForInterruption()
    {
        interruptWhenPossible = true;
    }

    public virtual void Precalculations(IReGoapAgent goapAgent, ReGoapState goalState)
    {
        agent = goapAgent;
    }

    public virtual GoapActionSettings GetSettings(IReGoapAgent goapAgent, ReGoapState goalState)
    {
        return settings;
    }

    public virtual ReGoapState GetPreconditions(ReGoapState goalState, IReGoapAction next = null)
    {
        return preconditions;
    }

    public virtual ReGoapState GetEffects(ReGoapState goalState, IReGoapAction next = null)
    {
        return effects;
    }

    public virtual int GetCost(ReGoapState goalState, IReGoapAction next = null)
    {
        return Cost;
    }

    public virtual bool CheckProceduralCondition(IReGoapAgent goapAgent, ReGoapState goalState, IReGoapAction next = null)
    {
        return true;
    }

    public virtual void Run(IReGoapAction previous, IReGoapAction next, ReGoapState goalState, Action<IReGoapAction> done,
        Action<IReGoapAction> fail)
    {
        interruptWhenPossible = false;
        enabled = true;
        doneCallback = done;
        failCallback = fail;

        previousAction = previous;
        nextAction = next;
    }

    public virtual void Exit(IReGoapAction next)
    {
        if (gameObject != null)
            enabled = false;
    }

    public virtual Dictionary<string, object> GetGenericValues()
    {
        return genericValues;
    }

    public virtual string GetName()
    {
        return Name;
    }

    public override string ToString()
    {
        return string.Format("GoapAction('{0}')", Name);
    }
}
