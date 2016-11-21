using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GoapMemory : MonoBehaviour, IReGoapMemory
{
    protected ReGoapState state;

    protected virtual void Awake()
    {
        state = new ReGoapState();
    }

    public virtual ReGoapState GetWorldState()
    {
        return state;
    }
}
