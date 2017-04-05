using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    public class PrimitiveResource : Resource
    {
        public float MinScalePercentage = 0.1f;
        private Vector3 startingScale;

        protected override void Awake ()
        {
            base.Awake();
            startingScale = transform.localScale;
        }

        public override void RemoveResource(float value)
        {
            base.RemoveResource(value);
            transform.localScale = startingScale * (MinScalePercentage + (1f - MinScalePercentage) * (Capacity / startingCapacity)); // scale down based on capacity
        }
    }
}