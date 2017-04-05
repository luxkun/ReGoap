using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    public class BankManager : MonoBehaviour
    {
        public static BankManager Instance;
        public Bank[] Banks;
        private int currentIndex;

        protected virtual void Awake()
        {
            if (Instance != null)
                throw new UnityException("[BankManager] Can have only one instance per scene.");
            Instance = this;
        }

        public Bank GetBank()
        {
            var result = Banks[currentIndex];
            currentIndex = currentIndex++ % Banks.Length;
            return result;
        }

        public int GetBanksCount()
        {
            return Banks.Length;
        }
    }
}
