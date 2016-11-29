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
        state = new ReGoapState();
        sensors = GetComponents<IReGoapSensor>();
        foreach (var sensor in sensors)
        {
            sensor.Init(this);
        }
    }

    protected virtual void Start()
    {
    }

    protected virtual void FixedUpdate()
    {
    }

    protected virtual void Update()
    {
    }
    #endregion

    public virtual ReGoapState GetWorldState()
    {
        return state;
    }
}
