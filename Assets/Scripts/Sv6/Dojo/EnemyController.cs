using System.Collections;
using UnityEngine;
using Sv6.Dojo.State;

namespace Sv6.Dojo
{
    public class EnemyController : MonoBehaviour
    {
        public float moveSpeed = 3.0f;
        public float detectionRange = 2.0f;
        public float attackRange = 0.5f;
        public float attackMoveSpeed = 2.0f;
        public float retreatDistance = 3.0f;
        public float retreatSpeed = 4.0f;
        public float chargeTime = 1.0f;
        public int damage = 10;
        public GameObject attackAreaPrefab;
        public Vector2 attackAreaOffset = Vector2.zero;

        public Vector2 minBounds; // Bottom-left corner of the map
        public Vector2 maxBounds; // Top-right corner of the map

        private Transform player;
        private Rigidbody2D body;
        private Animator animator;
        private string currentAnimation = "";
        private bool isAttacking = false;
        private bool isMovingToTarget = false;
        private Vector2 lastDirection = new Vector2(-1, -1);
        private Vector2 attackTargetPosition;
        private string attackDirection = "E";
        private Vector2 retreatTarget;
        private float chargeTimer;
        private float retreatTimer = 0f;
        private Vector2 attackLandingPosition;

        // State machine
        private enum EnemyState
        {
            Idle,
            Chase,
            Charge,
            Attack,
            Retreat,
            TakeHit,
            Die
        }

        private EnemyState currentState;

        // For restoring charge state after taking a hit
        private bool wasCharging = false;
        private float savedChargeTimer;

        // Reference to Health component
        private Health healthComponent;

        // Track if the enemy has attacked once, to see if we need special handling on the first attack.
        private bool hasAttackedOnce = false;

        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            body = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            currentState = EnemyState.Idle;

            // Example of initializing bounds dynamically (optional)
            minBounds = new Vector2(-3f, -1f);
            maxBounds = new Vector2(3.2f, 1f);

            // Get and subscribe to the OnEnemyDeath event
            healthComponent = GetComponent<Health>();
            if (healthComponent != null)
            {
                Health.OnEnemyDeath += HandleOnEnemyDeath;
            }
            else
            {
                Debug.LogError("Health component not found on EnemyController GameObject.");
            }
        }

        void OnDestroy()
        {
            // Unsubscribe from the event to prevent memory leaks
            if (healthComponent != null)
            {
                Health.OnEnemyDeath -= HandleOnEnemyDeath;
            }
        }

        void Update()
        {
            if (currentState == EnemyState.Die)
            {
                  // Ensure corpse stays within boundaries.
                ClampPositionToBounds();
                return;
            }

            switch (currentState)
            {
                case EnemyState.Idle:
                    HandleIdleState();
                    break;
                case EnemyState.Chase:
                    HandleChaseState();
                    break;
                case EnemyState.Charge:
                    HandleChargeState();
                    break;
                case EnemyState.Attack:
                    HandleAttackState();
                    break;
                case EnemyState.Retreat:
                    HandleRetreatState();
                    break;
                case EnemyState.TakeHit:
                    HandleTakeHitState();
                    break;
            }
            // After handling states for moving enemies, always clamp position
            ClampPositionToBounds();
        }

        private void ClampPositionToBounds()
        {
            Vector3 clampedPosition = new Vector3(
            Mathf.Clamp(transform.position.x, minBounds.x, maxBounds.x),
            Mathf.Clamp(transform.position.y, minBounds.y, maxBounds.y),
            transform.position.z);
            transform.position = clampedPosition;
        }

        void FixedUpdate()
        {
            if (isMovingToTarget)
            {
                MoveToAttackTarget();
            }
        }

        void HandleIdleState()
        {
            GoIdle();
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer < detectionRange)
            {
                currentState = EnemyState.Chase;
            }
        }

        void HandleChaseState()
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer < attackRange)
            {
                currentState = EnemyState.Charge;
                StartCharge();
            }
            else if (distanceToPlayer >= detectionRange)
            {
                currentState = EnemyState.Idle;
            }
            else
            {
                MoveTowardsPlayer();
            }
        }

        void StartCharge()
        {
            wasCharging = false; // Reset if already charging
            savedChargeTimer = chargeTime;

            attackDirection = player.position.x > transform.position.x ? "SE" : "SW";
            string chargeAnimation = "Charge_" + attackDirection;
            animator.Play(chargeAnimation);
            currentAnimation = chargeAnimation;

            chargeTimer = chargeTime;
            body.velocity = Vector2.zero;

            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            // Pre-calculate a tentative landing position (though we'll recalc later in Attack)
            attackLandingPosition = (Vector2)transform.position + directionToPlayer * attackRange;
        }

        void HandleChargeState()
        {
            // Resume from saved state if interrupted
            if (wasCharging)
            {
                chargeTimer = savedChargeTimer;
                wasCharging = false;
            }

            chargeTimer -= Time.deltaTime;
            if (chargeTimer <= 0)
            {
                currentState = EnemyState.Attack;
            }
        }

        void HandleAttackState()
        {
            if (!isAttacking)
            {
                Attack();
            }
        }

        void HandleRetreatState()
        {
            // Increment the retreat timer
            retreatTimer += Time.deltaTime;

            // Check if the enemy has been retreating for more than 2 seconds
            if (retreatTimer >= 2.0f)
            {
                // Transition to the next state (e.g., Chase)
                currentState = EnemyState.Chase;
                body.velocity = Vector2.zero;
                retreatTimer = 0f; // Reset the timer
                return;
            }

            if (Vector2.Distance(transform.position, retreatTarget) > 0.1f)
            {
                Vector2 retreatDirection = (retreatTarget - (Vector2)transform.position).normalized;
                body.velocity = retreatDirection * retreatSpeed;

                // Clamp position to ensure it stays within bounds
                Vector2 clampedPosition = new Vector2(
                    Mathf.Clamp(transform.position.x, minBounds.x, maxBounds.x),
                    Mathf.Clamp(transform.position.y, minBounds.y, maxBounds.y)
                );
                transform.position = clampedPosition;

                string animationName = GetAnimationName("Run", retreatDirection);
                if (currentAnimation != animationName)
                {
                    animator.Play(animationName);
                    currentAnimation = animationName;
                }
            }
            else
            {
                // Enemy has reached the retreat target
                currentState = EnemyState.Idle;
                body.velocity = Vector2.zero;
                retreatTimer = 0f; // Reset the timer
            }
        }

        void MoveTowardsPlayer()
        {
            Vector2 direction = (player.position - transform.position).normalized;
            body.velocity = direction * moveSpeed;

            lastDirection = direction;
            string animationName = GetAnimationName("Run", direction);

            if (currentAnimation != animationName)
            {
                animator.Play(animationName);
                currentAnimation = animationName;
            }
        }

        void Attack()
        {
            isAttacking = true;

            // Recalculate direction and attack landing position here to ensure it's fresh
            Vector2 directionToPlayer = player.position - transform.position;

            // If the enemy is extremely close to the player, ensure a minimum forward movement
            if (directionToPlayer.magnitude < 0.01f)
            {
                // Player is basically on top of enemy, choose a default direction
                directionToPlayer = Vector2.right; 
            }

            directionToPlayer.Normalize();

            // Ensure at least a small forward movement so it looks like it moves on the first attack
            float minForwardDistance = 0.3f; 
            float finalAttackDistance = Mathf.Max(attackRange, minForwardDistance);
            attackLandingPosition = (Vector2)transform.position + directionToPlayer * finalAttackDistance;

            attackTargetPosition = attackLandingPosition;

            attackDirection = player.position.x > transform.position.x ? "SE" : "SW";
            body.velocity = Vector2.zero;
            string attackAnimation = "Attack_" + attackDirection;
            animator.Play(attackAnimation);
            currentAnimation = attackAnimation;

            isMovingToTarget = true;
            CreateAttackArea();

            StartCoroutine(EndAttack());
        }

        void CreateAttackArea()
        {
            if (attackAreaPrefab != null)
            {
                Instantiate(attackAreaPrefab, transform.position + (Vector3)attackAreaOffset, Quaternion.identity, transform);
            }
            else
            {
                Debug.LogError("AttackAreaPrefab is not assigned in the EnemyController.");
            }
        }

        void MoveToAttackTarget()
        {
            float distanceToTarget = Vector2.Distance(transform.position, attackTargetPosition);

            if (distanceToTarget <= 0.1f)
            {
                body.velocity = Vector2.zero;
                isMovingToTarget = false;
            }
            else
            {
                Vector2 direction = (attackTargetPosition - (Vector2)transform.position).normalized;
                body.MovePosition(Vector2.MoveTowards(transform.position, attackTargetPosition, attackMoveSpeed * Time.fixedDeltaTime));
            }
        }

        private IEnumerator EndAttack()
        {
            // Wait a frame so the animator can update the state info properly
            yield return null;

            AnimatorStateInfo animStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float attackAnimationLength = animStateInfo.length;

            yield return new WaitForSeconds(attackAnimationLength);
            isAttacking = false;
            hasAttackedOnce = true;
            currentState = EnemyState.Retreat;
            CalculateRetreatTarget();
        }

        void CalculateRetreatTarget()
        {
            Vector2 retreatDirection = ((Vector2)transform.position - (Vector2)player.position).normalized;
            retreatTarget = (Vector2)transform.position + retreatDirection * retreatDistance;
        }

        void HandleTakeHitState()
        {
            // Save current state for resuming charge
            if (currentState == EnemyState.Charge)
            {
                wasCharging = true;
                savedChargeTimer = chargeTimer;
            }

            body.velocity = Vector2.zero;

            string hitDirection = lastDirection.x > 0 ? "SE" : "SW";
            string hitAnimation = "TakeHit_" + hitDirection;
            animator.Play(hitAnimation);
            currentAnimation = hitAnimation;

            StartCoroutine(WaitForHitAnimation());
        }

        private IEnumerator WaitForHitAnimation()
        {
            AnimatorStateInfo animStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(animStateInfo.length);

            // Resume charge or transition to retreat
            if (wasCharging)
            {
                currentState = EnemyState.Charge;
            }
            else
            {
                currentState = EnemyState.Retreat;
                CalculateRetreatTarget();
            }
        }

        public void TakeHit()
        {
            // Only handle take hit if not already dead
            if (currentState != EnemyState.Die)
            {
                currentState = EnemyState.TakeHit;
                CameraFreeze cameraFreeze = FindObjectOfType<CameraFreeze>();
                if (cameraFreeze != null)
                {
                    cameraFreeze.Freeze();
                }
                SoundManager.PlaySound("DrogHit");
            }
        }

        void GoIdle()
        {
            body.velocity = Vector2.zero;
            string idleAnimationName = GetAnimationName("Idle", lastDirection);

            if (currentAnimation != idleAnimationName)
            {
                animator.Play(idleAnimationName);
                currentAnimation = idleAnimationName;
            }
        }

        private string GetAnimationName(string animationType, Vector2 direction)
        {
            string directionSuffix = GetDirectionSuffix(direction);
            return animationType + "_" + directionSuffix;
        }

        public static string GetDirectionSuffix(Vector2 direction)
        {
            if (direction.x > 0 && direction.y > 0) return "NE";
            if (direction.x < 0 && direction.y > 0) return "NW";
            if (direction.x > 0 && direction.y < 0) return "SE";
            return "SW";
        }

        // Event handler for global enemy death
        private void HandleOnEnemyDeath()
        {
            // Check if this enemy is dead
            if (healthComponent != null && healthComponent.IsDead)
            {
                EnterDieState();
            }
        }

        // Method to handle entering the Die state
        private void EnterDieState()
        {
            if (currentState != EnemyState.Die)
            {
                currentState = EnemyState.Die;

                // Stop all ongoing coroutines to prevent state changes
                StopAllCoroutines();

                StartCoroutine(HandleDieState());
            }
        }

        private IEnumerator HandleDieState()
        {
            // Ensure velocity is zero
            body.velocity = Vector2.zero;
            
            //death SFX
            SoundManager.PlaySound("DrogDie");

            //get rid of shadow
            Transform shadowTransform = transform.Find("shadow");
            if (shadowTransform != null)
            {
                Destroy(shadowTransform.gameObject);
            }
            else
            {
                Debug.LogWarning($"Shadow object not found on {gameObject.name}.");
            }

            // Determine direction to choose the correct Die animation
            string dieDirection = lastDirection.x > 0 ? "SE" : "SW";
            string dieAnimation = "Die_" + dieDirection;
            animator.Play(dieAnimation);
            currentAnimation = dieAnimation;

            // Wait for Die animation to finish
            AnimatorStateInfo dieAnimState = animator.GetCurrentAnimatorStateInfo(0);
            float dieAnimLength = dieAnimState.length;
            yield return new WaitForSeconds(dieAnimLength);

            // Play Dead animation (single frame)
            string deadAnimation = "Dead_" + dieDirection;
            animator.Play(deadAnimation);
            currentAnimation = deadAnimation;

            // Wait for 1 second while staying on Dead animation
            yield return new WaitForSeconds(1.0f);

            // Finally, destroy the enemy GameObject
            Destroy(gameObject);
        }
    }
}
