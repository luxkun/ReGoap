using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ReGoapSensor<T, W> : MonoBehaviour, IReGoapSensor<T, W>
{
    protected IReGoapMemory<T, W> memory;
    public virtual void Init(IReGoapMemory<T, W> memory)
    {
        this.memory = memory;
    }

    public virtual IReGoapMemory<T, W> GetMemory()
    {
        return memory;
    }

    public virtual void UpdateSensor()
    {

    }
}
