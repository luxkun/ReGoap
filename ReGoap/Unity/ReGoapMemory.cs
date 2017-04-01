using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ReGoapMemory<T, W> : MonoBehaviour, IReGoapMemory<T, W>
{
    protected ReGoapState<T, W> state;
    private IReGoapSensor<T, W>[] sensors;

    #region UnityFunctions
    protected virtual void Awake()
    {
        state = ReGoapState<T, W>.Instantiate();
        sensors = GetComponents<IReGoapSensor<T, W>>();
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

    public virtual ReGoapState<T, W> GetWorldState()
    {
        return state;
    }
}
