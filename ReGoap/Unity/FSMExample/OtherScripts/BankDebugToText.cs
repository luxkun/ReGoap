using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BankDebugToText : MonoBehaviour
{
    public Text Text;
    private Bank bank;

    void Awake()
    {
        bank = GetComponent<Bank>();
    }

	void FixedUpdate ()
	{
	    var result = "";
	    foreach (var pair in bank.GetResources())
	    {
	        result += string.Format("{0}: {1}\n", pair.Key, pair.Value);
	    }
	    Text.text = result;
	}
}
