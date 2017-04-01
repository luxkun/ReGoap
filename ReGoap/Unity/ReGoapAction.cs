using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class ReGoapAction<T, W> : MonoBehaviour, IReGoapAction<T, W>
{
    public string Name = "GoapAction";

    protected ReGoapState<T, W> preconditions;
    protected ReGoapState<T, W> effects;
    public float Cost = 1;

    protected Action<IReGoapAction<T, W>> doneCallback;
    protected Action<IReGoapAction<T, W>> failCallback;
    protected IReGoapAction<T, W> previousAction;
    protected IReGoapAction<T, W> nextAction;

    protected IReGoapAgent<T, W> agent;
    protected Dictionary<string, object> genericValues;
    protected bool interruptWhenPossible;

    protected IReGoapActionSettings<T, W> settings = null;

    #region UnityFunctions
    protected virtual void Awake()
    {
        enabled = false;

        effects = ReGoapState<T, W>.Instantiate();
        preconditions = ReGoapState<T, W>.Instantiate();

        genericValues = new Dictionary<string, object>();
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

    public virtual void PostPlanCalculations(IReGoapAgent<T, W> goapAgent)
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

    public virtual void Precalculations(IReGoapAgent<T, W> goapAgent, ReGoapState<T, W> goalState)
    {
        agent = goapAgent;
    }

    public virtual IReGoapActionSettings<T, W> GetSettings(IReGoapAgent<T, W> goapAgent, ReGoapState<T, W> goalState)
    {
        return settings;
    }

    public virtual ReGoapState<T, W> GetPreconditions(ReGoapState<T, W> goalState, IReGoapAction<T, W> next = null)
    {
        return preconditions;
    }

    public virtual ReGoapState<T, W> GetEffects(ReGoapState<T, W> goalState, IReGoapAction<T, W> next = null)
    {
        return effects;
    }

    public virtual float GetCost(ReGoapState<T, W> goalState, IReGoapAction<T, W> next = null)
    {
        return Cost;
    }

    public virtual bool CheckProceduralCondition(IReGoapAgent<T, W> goapAgent, ReGoapState<T, W> goalState, IReGoapAction<T, W> next = null)
    {
        return true;
    }

    public virtual void Run(IReGoapAction<T, W> previous, IReGoapAction<T, W> next, IReGoapActionSettings<T, W> settings,
        ReGoapState<T, W> goalState, Action<IReGoapAction<T, W>> done, Action<IReGoapAction<T, W>> fail)
    {
        interruptWhenPossible = false;
        enabled = true;
        doneCallback = done;
        failCallback = fail;

        previousAction = previous;
        nextAction = next;
    }

    public virtual void Exit(IReGoapAction<T, W> next)
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
