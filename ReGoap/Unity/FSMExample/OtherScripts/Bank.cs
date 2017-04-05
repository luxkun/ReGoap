using System.Collections.Generic;
using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    public class Bank : MonoBehaviour
    {
        private ResourcesBag bankBag;

        void Awake()
        {
            bankBag = gameObject.AddComponent<ResourcesBag>();
        }

        public float GetResource(string resourceName)
        {
            return bankBag.GetResource(resourceName);
        }

        public Dictionary<string, float> GetResources()
        {
            return bankBag.GetResources();
        }

        public bool AddResource(ResourcesBag resourcesBag, string resourceName, float value = 1f)
        {
            if (resourcesBag.GetResource(resourceName) >= value)
            {
                resourcesBag.RemoveResource(resourceName, value);
                bankBag.AddResource(resourceName, value);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
