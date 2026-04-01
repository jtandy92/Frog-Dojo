using UnityEngine;
namespace Sv6.Dojo{

public class EnemyAttackArea : MonoBehaviour
{
    public int damageAmount = 10; // Damage to deal
    private bool hasDamaged = false; // Ensure damage is applied only once per attack
    private PlayerController playerController;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasDamaged && other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();
            PlayerController playerController = other.GetComponent<PlayerController>();
            
            if (playerHealth != null && playerController != null)
            {
                playerHealth.Damage(damageAmount);
                playerController.HandleHit(damageAmount);
                hasDamaged = true; // Prevent multiple damage applications
                //Debug.Log("Player damaged by AttackArea.");
            }
            else 
            {
                Debug.Log("Player's health is null");
            }
        }
    }

    // Destroy the attack area after a certain time
    private void Start()
    {
        //Debug.Log("AttackArea created.");
        Destroy(gameObject, 0.5f); // Adjust the lifetime as needed
    }

    private void OnDestroy()
    {
        //Debug.Log("AttackArea destroyed.");
        // Optionally reset hasDamaged if you want to allow damage again in future instances
    }
}

}