using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BankSensor : GoapSensor
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

        var nearestBank = Utilities.GetNearest(transform.position, banks);
        worldState.Set("nearestBank", nearestBank);
        worldState.Set("nearestBankPosition", nearestBank != null ? nearestBank.transform.position : Vector3.zero);
        if (nearestBank != null &&
            (transform.position - nearestBank.transform.position).sqrMagnitude < MinPowDistanceToBeNear)
        {
            worldState.Set("isAtTransform", nearestBank.transform);
        }
        else if (nearestBank != null && worldState.Get<Transform>("isAtTransform") == nearestBank.transform)
        {
            worldState.Set<Transform>("isAtTransform", null);
        }
    }
}
