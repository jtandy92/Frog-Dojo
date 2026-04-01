using UnityEngine;
using System;

namespace Sv6.Dojo
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int health;

        // Reference to a HealthBar not necessarily attached to the player
        [SerializeField] private HealthBar healthBar;

        public static event Action OnEnemyDeath;
        public event System.Action OnPlayerDeath;

        // Public property to check if the entity is dead
        public bool IsDead => health <= 0;

        private void Start()
        {
            // Initialize health
            health = maxHealth;

            // If healthBar is not assigned via Inspector, try to find one in the scene
            if (healthBar == null)
            {
                healthBar = FindObjectOfType<HealthBar>();
                if (healthBar == null)
                {
                    Debug.LogError("No HealthBar found in the scene. Please assign one in the inspector or add it to the scene.");
                    return;
                }
            }

            // Initialize the health bar display
            UpdateHealthBar();
        }

        public void Damage(int amount)
        {
            if (amount > 0)
            {
                health -= amount;
                if (health < 0) health = 0;

                UpdateHealthBar();
                if (health <= 0) Die();
            }
        }

        public void Heal(int amount)
        {
            if (amount > 0)
            {
                health += amount;
                if (health > maxHealth) health = maxHealth;

                UpdateHealthBar();
            }
        }

        private void UpdateHealthBar()
        {
            if (healthBar != null)
            {
                float healthPercentage = (float)health / maxHealth;
                healthBar.SetHealth(healthPercentage);
            }
        }

        private void Die()
        {
            if (CompareTag("Player"))
            {
                OnPlayerDeath?.Invoke();
                Debug.Log($"{gameObject.name} (Player) has died.");
            }
            else if (CompareTag("Enemy"))
            {
                OnEnemyDeath?.Invoke();
                Debug.Log($"{gameObject.name} (Enemy) has died.");
            }
            else
            {
                Debug.LogWarning($"{gameObject.name} has no designated tag for death handling.");
            }

            // The specific destruction or reset logic can go here or in a controller handling death
        }
    }
}
