using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public PolygonCollider2D attackArea;  // Polygon collider to define attack range
    public int attackDamage = 10;         // Damage dealt per attack

    public Animator animator; 

    private void Start()
    {

         // Ensure the animator is assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

    }
    private void Update()
    {
        // Input can be customized per character type if needed.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        animator.Play("Attack1East");
    }

 
}