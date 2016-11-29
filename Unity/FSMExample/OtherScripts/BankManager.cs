using UnityEngine;
using System.Collections;

public class BankManager : MonoBehaviour
{
    public static BankManager instance;
    public Bank[] banks;
    private int currentIndex;

    protected virtual void Awake()
    {
        if (instance != null)
            throw new UnityException("[BankManager] Can have only one instance per scene.");
        instance = this;
    }

    public Bank GetBank()
    {
        var result = banks[currentIndex];
        currentIndex = currentIndex++ % banks.Length;
        return result;
    }

    public int GetBanksCount()
    {
        return banks.Length;
    }
}
