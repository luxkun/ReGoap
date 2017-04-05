using ReGoap.Core;
using UnityEngine;

namespace ReGoap.Unity
{
    public class ReGoapMemory<T, W> : MonoBehaviour, IReGoapMemory<T, W>
    {
        protected ReGoapState<T, W> state;
        private IReGoapSensor<T, W>[] sensors;

        public float SensorsUpdateDelay = 0.3f;
        private float sensorsUpdateCooldown;

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

        protected virtual void Update()
        {
            if (Time.time > sensorsUpdateCooldown)
            {
                sensorsUpdateCooldown = Time.time + SensorsUpdateDelay;

                foreach (var sensor in sensors)
                {
                    sensor.UpdateSensor();
                }
            }
        }
        #endregion

        public virtual ReGoapState<T, W> GetWorldState()
        {
            return state;
        }
    }
}
