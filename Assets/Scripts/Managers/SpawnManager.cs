// SpawnManager.cs
using System.Collections;
using UnityEngine;

namespace Sv6.Dojo
{
    public class SpawnManager : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [Tooltip("Enemy prefab to spawn.")]
        public GameObject enemyPrefab;

        [Tooltip("Array of spawn points where enemies can spawn.")]
        public Transform[] spawnPoints;

        [Tooltip("Total number of waves.")]
        public int totalWaves = 10;

        [Tooltip("Number of enemies per wave. Set the size to the total number of waves.")]
        public int[] enemiesPerWave = new int[10];

        [Header("Wave Timing Settings")]
        [Tooltip("Time delay between the end of one wave and the start of the next.")]
        public float timeBetweenWaves = 5f;

        [Tooltip("Time delay between spawning each enemy within a wave.")]
        public float spawnInterval = 1f;

        private int currentWave = 0;
        private int enemiesToSpawn;
        private int enemiesKilled;

        private bool isWaveActive = false;

        private void OnEnable()
        {
            // Subscribe to the enemy death event
            Health.OnEnemyDeath += OnEnemyDeath;
        }

        private void OnDisable()
        {
            // Unsubscribe from the enemy death event to prevent memory leaks
            Health.OnEnemyDeath -= OnEnemyDeath;
        }

        void Start()
        {
            if (spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points assigned to SpawnManager.");
                return;
            }

            if (enemiesPerWave.Length != totalWaves)
            {
                Debug.LogError("The size of the 'enemiesPerWave' array must match 'totalWaves'.");
                return;
            }

            StartCoroutine(StartWaves());
        }

        IEnumerator StartWaves()
        {
            while (currentWave < totalWaves)
            {
                currentWave++;

                // Get the number of enemies for the current wave
                enemiesToSpawn = enemiesPerWave[currentWave - 1];
                enemiesKilled = 0;
                isWaveActive = true;

                Debug.Log($"[SpawnManager] Wave {currentWave} started with {enemiesToSpawn} enemies.");

                StartCoroutine(SpawnEnemies());

                // Wait until the wave is completed
                yield return new WaitUntil(() => !isWaveActive);

                Debug.Log($"[SpawnManager] Wave {currentWave} completed.");

                yield return new WaitForSeconds(timeBetweenWaves);
            }

            Debug.Log("[SpawnManager] All waves completed!");
            // Optional: Trigger end-game logic here
        }

        IEnumerator SpawnEnemies()
        {
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        void SpawnEnemy()
        {
            int spawnIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[spawnIndex];
            Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        }

        public int GetCurrentWave()
        {
            return currentWave;
        }

        void OnEnemyDeath()
        {
            enemiesKilled++;
            Debug.Log($"[SpawnManager] Enemy killed. Total killed in wave {currentWave}: {enemiesKilled}/{enemiesToSpawn}");

            // Check if the current wave is completed
            if (enemiesKilled >= enemiesToSpawn)
            {
                isWaveActive = false;
            }
        }
    }
}
