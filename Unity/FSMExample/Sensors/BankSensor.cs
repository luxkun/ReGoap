using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BankSensor : GoapSensor
{
    private Dictionary<Bank, Vector3> banks;

    public float minPowDistanceToBeNear = 1f;

    void Start()
    {
        banks = new Dictionary<Bank, Vector3>(BankManager.instance.banks.Length);
        foreach (var bank in BankManager.instance.banks)
        {
            banks[bank] = bank.transform.position;
        }
    }

    public override void UpdateSensor()
    {
        var worldState = memory.GetWorldState();
        worldState.Set("seeBank", BankManager.instance != null && BankManager.instance.banks.Length > 0);

        var nearestBank = Utilities.GetNearest(transform.position, banks);
        worldState.Set("nearestBank", nearestBank);
        if (nearestBank != null &&
            (transform.position - nearestBank.transform.position).sqrMagnitude < minPowDistanceToBeNear)
        {
            worldState.Set("isAtTransform", nearestBank.transform);
        }
        else if (nearestBank != null && worldState.Get<Transform>("isAtTransform") == nearestBank.transform)
        {
            worldState.Set<Transform>("isAtTransform", null);
        }
    }
}
