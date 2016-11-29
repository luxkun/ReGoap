using UnityEngine;
using System.Collections;

public class PrimitiveResource : Resource
{
    public float minScalePercentage = 0.1f;
    private Vector3 startingScale;

    protected override void Awake ()
    {
	    startingScale = transform.localScale;
	}

    public override void RemoveResource(float value)
    {
        base.RemoveResource(value);
        startingScale = startingScale * minScalePercentage + (1f - minScalePercentage) * startingScale *(capacity/startingCapacity); // scale down based on capacity
    }
}