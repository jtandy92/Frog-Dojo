using System.Collections;
using UnityEngine;

namespace Sv6.Dojo
{
    public class AttackArea : MonoBehaviour
    {
        private int damage;

        // Setup method to assign damage when the attack area is created
        public void Setup(int damageAmount)
        {
            damage = damageAmount;
            Debug.Log($"Attack area set up with {damage} damage.");
        }

        private void Start()
        {
            // Automatically destroy the attack area after a short delay
            StartCoroutine(DestroySelf());
        }

        // Detect collision with enemies using a trigger
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy"))
            {
                Health enemyHealth = collision.GetComponent<Health>();
                EnemyController enemyController = collision.GetComponent<EnemyController>();
                Debug.Log("Enemy detected inside attack area: " + collision.name);

                if (enemyHealth != null)
                {
                    Debug.Log($"Applying {damage} damage to {collision.name}.");
                    enemyHealth.Damage(damage); // Apply damage

                    // Trigger the TakeHit state if the enemy has an EnemyController
                    if (enemyController != null)
                    {
                        enemyController.TakeHit();
                        Debug.Log($"{collision.name} entered TakeHit state.");
                    }

                    // Apply pushback
                    Rigidbody2D enemyRb = collision.GetComponent<Rigidbody2D>();
                    if (enemyRb != null)
                    {
                        Vector2 pushbackDirection = (collision.transform.position - transform.position).normalized; 
                        float pushbackForce = 0.5f; // Adjust the pushback force as needed
                        enemyRb.AddForce(pushbackDirection * pushbackForce, ForceMode2D.Impulse);

                        // Set temporary drag to slow down the enemy
                        float temporaryDrag = 2f; 
                        enemyRb.drag = temporaryDrag;

                        Debug.Log($"Pushback applied to {collision.name} in direction {pushbackDirection} with force {pushbackForce}.");
                    }
                    else
                    {
                        Debug.LogError($"No Rigidbody2D component found on {collision.name}.");
                    }
                }
                else
                {
                    Debug.LogError($"No Health component found on {collision.name}.");
                }
            }
            else
            {
                Debug.Log($"Non-enemy object entered the attack area: {collision.name}");
            }
        }

        private IEnumerator DestroySelf()
        {
            yield return new WaitForSeconds(0.5f); // Adjust the lifespan of the attack area
            Destroy(gameObject);
            Debug.Log("Attack area destroyed.");
        }
    }
}
