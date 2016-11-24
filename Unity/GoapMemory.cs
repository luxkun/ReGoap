using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GoapMemory : MonoBehaviour, IReGoapMemory
{
    protected ReGoapState state;
    private IReGoapSensor[] sensors;

    protected virtual void Awake()
    {
        state = new ReGoapState();
        sensors = GetComponents<IReGoapSensor>();
        foreach (var sensor in sensors)
        {
            sensor.Init(this);
        }
    }

    public virtual ReGoapState GetWorldState()
    {
        return state;
    }
}
