using UnityEngine;
using System.Collections;

public class PrimitiveResource : Resource
{
    public float MinScalePercentage = 0.1f;
    private Vector3 startingScale;

    protected override void Awake ()
    {
	    startingScale = transform.localScale;
	}

    public override void RemoveResource(float value)
    {
        base.RemoveResource(value);
        startingScale = startingScale * MinScalePercentage + (1f - MinScalePercentage) * startingScale *(Capacity/startingCapacity); // scale down based on capacity
    }
}