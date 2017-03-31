using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GoapMemory : MonoBehaviour, IReGoapMemory
{
    protected ReGoapState state;
    private IReGoapSensor[] sensors;

    #region UnityFunctions
    protected virtual void Awake()
    {
        state = ReGoapState.Instantiate();
        sensors = GetComponents<IReGoapSensor>();
        foreach (var sensor in sensors)
        {
            sensor.Init(this);
        }
    }

    void OnDestroy()
    {
        state.Recycle();
    }

    protected virtual void Start()
    {
    }

    protected virtual void FixedUpdate()
    {
    }

    protected virtual void Update()
    {
        foreach (var sensor in sensors)
        {
            sensor.UpdateSensor();
        }
    }
    #endregion

    public virtual ReGoapState GetWorldState()
    {
        return state;
    }
}
