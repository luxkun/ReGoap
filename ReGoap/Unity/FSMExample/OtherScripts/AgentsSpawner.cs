using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{
    public class AgentsSpawner : MonoBehaviour
    {
        public int BuildersCount;
        private int spawnedBuilders;
        public GameObject BuilderPrefab;

        public float DelayBetweenSpawns = 0.1f;
        public int AgentsPerSpawn = 100;
        private float spawnCooldown;

        void Awake()
        {
        }

        void Update()
        {
            if (Time.time >= spawnCooldown && spawnedBuilders < BuildersCount)
            {
                spawnCooldown = Time.time + DelayBetweenSpawns;
                for (int i = 0; i < AgentsPerSpawn && spawnedBuilders < BuildersCount; i++)
                {
                    var gameObj = Instantiate(BuilderPrefab);
                    gameObj.transform.SetParent(transform);

                    spawnedBuilders++;
                }
            }
        }
    }
}
