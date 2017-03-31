using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GoapSensor : MonoBehaviour, IReGoapSensor
{
    protected IReGoapMemory memory;
    public virtual void Init(IReGoapMemory memory)
    {
        this.memory = memory;
    }

    public virtual IReGoapMemory GetMemory()
    {
        return memory;
    }

    public virtual void UpdateSensor()
    {

    }
}
