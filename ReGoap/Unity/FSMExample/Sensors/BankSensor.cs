using System.Collections.Generic;
using ReGoap.Unity.FSMExample.OtherScripts;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.Sensors
{
    public class BankSensor : ReGoapSensor<string, object>
    {
        private Dictionary<Bank, Vector3> banks;

        public float MinPowDistanceToBeNear = 1f;

        void Start()
        {
            banks = new Dictionary<Bank, Vector3>(BankManager.Instance.Banks.Length);
            foreach (var bank in BankManager.Instance.Banks)
            {
                banks[bank] = bank.transform.position;
            }
        }

        public override void UpdateSensor()
        {
            var worldState = memory.GetWorldState();
            worldState.Set("seeBank", BankManager.Instance != null && BankManager.Instance.Banks.Length > 0);

            var nearestBank = OtherScripts.Utilities.GetNearest(transform.position, banks);
            worldState.Set("nearestBank", nearestBank);
            worldState.Set("nearestBankPosition",
                (Vector3?) (nearestBank != null ? nearestBank.transform.position : Vector3.zero));
        }
    }
}