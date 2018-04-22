using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Unity.FSMExample.OtherScripts;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.Sensors
{
    public class PositionSensor : ReGoapSensor<string, object>
    {
        private Dictionary<Bank, Vector3> banks;

        public override void Init(IReGoapMemory<string, object> memory)
        {
            base.Init(memory);
            var state = memory.GetWorldState();
            state.Set("startPosition", transform.position);
        }

        public override void UpdateSensor()
        {
            var state = memory.GetWorldState();
            state.Set("startPosition", transform.position);
        }
    }
}