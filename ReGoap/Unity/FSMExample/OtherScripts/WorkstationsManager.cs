using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    public class WorkstationsManager : MonoBehaviour
    {
        public static WorkstationsManager Instance;
        public Workstation[] Workstations;
        private int currentIndex;

        protected virtual void Awake()
        {
            if (Instance != null)
                throw new UnityException("[WorkstationsManager] Can have only one instance per scene.");
            Instance = this;
        }

        public Workstation GetWorkstation()
        {
            var result = Workstations[currentIndex];
            currentIndex = currentIndex++ % Workstations.Length;
            return result;
        }
    }
}
