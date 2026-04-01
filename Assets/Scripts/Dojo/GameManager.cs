using UnityEngine;
using TMPro;

namespace Sv6.Dojo
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("UI References")]
        [Tooltip("Assign the TMP_Text component that displays the wave number.")]
        [SerializeField] private TMP_Text waveNumberText;

        [Tooltip("Assign the TMP_Text component that displays the player's points.")]
        [SerializeField] private TMP_Text playerPointsText;

        [Header("References")]
        [Tooltip("Reference to the SpawnManager that controls wave progression.")]
        [SerializeField] private SpawnManager spawnManager;

        private int playerPoints = 0;

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            // Subscribe to the enemy death event
            Health.OnEnemyDeath += HandleEnemyDeath;
        }

        private void OnDisable()
        {
            // Unsubscribe from the enemy death event
            Health.OnEnemyDeath -= HandleEnemyDeath;
        }

        private void Start()
        {
            // If not assigned in the Inspector, try to find a SpawnManager in the scene
            if (spawnManager == null)
            {
                spawnManager = FindObjectOfType<SpawnManager>();
                if (spawnManager == null)
                {
                    Debug.LogError("[GameManager] No SpawnManager found in the scene.");
                }
            }

            UpdateUI();
        }

        private void Update()
        {
            // Continuously update UI to reflect current wave number from spawnManager
            UpdateUI();
        }

        /// <summary>
        /// Handles logic when an enemy is killed: increase points and update UI.
        /// </summary>
        private void HandleEnemyDeath()
        {
            playerPoints += 100;
            UpdateUI();
        }

        /// <summary>
        /// Updates the UI elements to reflect the current game state.
        /// Wave number is retrieved from the SpawnManager.
        /// </summary>
        private void UpdateUI()
        {
            if (spawnManager != null && waveNumberText != null)
            {
                waveNumberText.text = $"Wave: {spawnManager.GetCurrentWave()}";
            }

            if (playerPointsText != null)
            {
                playerPointsText.text = $"Points: {playerPoints}";
            }
        }

        /// <summary>
        /// Resets the game state.
        /// </summary>
        public void ResetGame()
        {
            playerPoints = 0;
            UpdateUI();
        }
    }
}
