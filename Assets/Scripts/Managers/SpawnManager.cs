using UnityEngine;

namespace MobaPrototype
{
    /// <summary>
    /// Spawns waves of lane creeps for both teams at a fixed interval and sends
    /// them walking down the lane toward the opposing base. Spawned creeps are
    /// parented under the Allies / Enemies hierarchy containers to keep the
    /// scene hierarchy tidy while the match is running.
    /// </summary>
    public class SpawnManager : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject allyCreepPrefab;
        public GameObject enemyCreepPrefab;

        [Header("Spawn points")]
        public Transform allySpawnPoint;
        public Transform enemySpawnPoint;

        [Header("Lane destinations (opposing base)")]
        public Transform allyLaneDestination;
        public Transform enemyLaneDestination;

        [Header("Hierarchy containers")]
        public Transform alliesContainer;
        public Transform enemiesContainer;

        [Header("Wave settings")]
        public float spawnInterval = 8f;
        public int creepsPerWave = 5;
        public float creepSpacing = 1.5f;

        private float timer;

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                return;

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                SpawnWave();
                timer = spawnInterval;
            }
        }

        private void SpawnWave()
        {
            for (int i = 0; i < creepsPerWave; i++)
            {
                SpawnCreep(allyCreepPrefab, allySpawnPoint, allyLaneDestination, alliesContainer, i);
                SpawnCreep(enemyCreepPrefab, enemySpawnPoint, enemyLaneDestination, enemiesContainer, i);
            }
        }

        private void SpawnCreep(GameObject prefab, Transform spawnPoint, Transform destination, Transform container, int index)
        {
            if (prefab == null || spawnPoint == null) return;

            float centeredOffset = (index - (creepsPerWave - 1) / 2f) * creepSpacing;
            Vector3 spawnPos = spawnPoint.position + new Vector3(centeredOffset, 0f, 0f);
            GameObject creepGO = Instantiate(prefab, spawnPos, spawnPoint.rotation, container);

            LaneUnitAI ai = creepGO.GetComponent<LaneUnitAI>();
            if (ai != null && destination != null)
                ai.Init(destination);
        }
    }
}
