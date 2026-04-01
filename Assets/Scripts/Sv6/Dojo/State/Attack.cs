using Sv6.Dojo;
using UnityEngine;
using System.Collections.Generic;

namespace Sv6.Dojo.State
{
    public class Attack : Base
    {
        private Vector2 attackDirection;
        private bool attacking = false;
        private float timer = 0f;

        // Instead of having separate prefabs for each direction, use one base prefab.
        [SerializeField] private GameObject baseAttackArea;

        // We'll keep a reference to the currently-instantiated attack area so we can destroy it later.
        [SerializeField] private GameObject currentAttackArea;

        public int damage = 10; // Define the damage dealt by the attack

        // Combo variables
        private int currentComboStep = 1;
        private const int maxComboSteps = 3;
        private bool nextAttackQueued = false;
        private bool comboWindowOpened = false;
        private float comboWindowStartTime = 0.2f; // Time after which combo window opens
        private float comboWindowDuration = 0.15f; // Duration of combo window

        // Attack movement variables
        [SerializeField] private List<float> comboMovementSpeeds = new List<float>();

        // Animation duration tracking
        private float currentAnimationLength = 0f;

        public void SetAttackDirection(Vector2 direction)
        {
            attackDirection = direction.normalized;
        }

        public override void Enter()
        {
            base.Enter();

            // Initialize combo variables
            currentComboStep = 1;
            nextAttackQueued = false;
            comboWindowOpened = false;
            timer = 0f;

            attacking = true;

            // Default to down if attack direction is invalid
            if (attackDirection == Vector2.zero)
            {
                attackDirection = Vector2.down;
            }

            // Apply movement in the attack direction
            ApplyAttackMovement();

            // Play the initial attack animation and retrieve its length
            string animationName = GetAttackAnimationName();
            PlayAttackAnimation(animationName);
            currentAnimationLength = GetAnimationLength(animationName);

            if (currentAnimationLength <= 0f)
            {
                Debug.LogError($"Animation length for {animationName} is invalid. Ensure the animation exists and is correctly named.");
            }
            else
            {
                Debug.Log($"Playing attack animation: {animationName}, Length: {currentAnimationLength}s");
            }

            // Start the attack logic
            AttackAction();
        }

        public override void Do()
        {
            if (attacking)
            {
                timer += Time.deltaTime;

                // Open combo window after some time
                if (!comboWindowOpened && timer >= comboWindowStartTime)
                {
                    comboWindowOpened = true;
                    Debug.Log("Combo window opened.");
                }

                // If a next attack is queued during the combo window, trigger the next combo step
                if (comboWindowOpened && nextAttackQueued && currentComboStep < maxComboSteps)
                {
                    currentComboStep++;
                    nextAttackQueued = false;
                    comboWindowOpened = false;
                    timer = 0f;

                    string nextAnimationName = GetAttackAnimationName();
                    PlayAttackAnimation(nextAnimationName);
                    currentAnimationLength = GetAnimationLength(nextAnimationName);

                    ApplyAttackMovement();
                    AttackAction();
                }

                // If current animation is done
                if (timer >= currentAnimationLength)
                {
                    // If there's still a queued attack and we haven't reached max combos
                    if (currentComboStep < maxComboSteps && nextAttackQueued)
                    {
                        currentComboStep++;
                        nextAttackQueued = false;
                        timer = 0f;

                        string nextAnimationName = GetAttackAnimationName();
                        PlayAttackAnimation(nextAnimationName);
                        currentAnimationLength = GetAnimationLength(nextAnimationName);

                        ApplyAttackMovement();
                        AttackAction();
                    }
                    else
                    {
                        // End of the combo or no queued attacks
                        attacking = false;

                        // Destroy the attack area
                        if (currentAttackArea != null)
                        {
                            DestroyImmediate(currentAttackArea, true);
                            currentAttackArea = null;
                            Debug.Log("Attack area destroyed after attack completion.");
                        }

                        _rigidbody.velocity = Vector2.zero;
                        Debug.Log("Player velocity reset to zero after attack.");

                        _isComplete = true;
                        Debug.Log("Attack state marked as complete.");
                    }
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
            attacking = false;
            timer = 0f;

            // Cleanup if we exit mid-attack
            if (currentAttackArea != null)
            {
                DestroyImmediate(currentAttackArea, true);
                currentAttackArea = null;
                Debug.Log("Attack area destroyed on exit.");
            }

            _rigidbody.velocity = Vector2.zero;
            Debug.Log("Player velocity reset to zero on exit.");
        }

        private void AttackAction()
        {
            // Clean up old hitbox if one still exists
            if (currentAttackArea != null)
            {
                DestroyImmediate(currentAttackArea, true);
                currentAttackArea = null;
            }

            if (baseAttackArea != null)
            {
                // Calculate the angle so the hitbox faces the mouse direction
                float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;

                // Instantiate the hitbox as a child of the player
                GameObject instantiatedAttackArea = Instantiate(baseAttackArea, _playerController.transform);
                instantiatedAttackArea.transform.localPosition = Vector3.zero;
                instantiatedAttackArea.transform.localRotation = Quaternion.Euler(0f, 0f, angle);

                // Setup the damage on this area
                instantiatedAttackArea.GetComponent<AttackArea>().Setup(damage);
                currentAttackArea = instantiatedAttackArea;

                Debug.Log($"Attack area created: {currentAttackArea.name} at position {currentAttackArea.transform.position}, rotated to {angle} degrees.");
                SoundManager.PlaySound("AttackSound");
            }
            else
            {
                Debug.LogError("baseAttackArea prefab reference is missing in the Attack state.");
            }
        }

        private string GetAttackAnimationName()
        {
            // Determine the direction name for the attack based on attackDirection
            Vector2 normalizedDirection = attackDirection.normalized;
            if (Mathf.Abs(normalizedDirection.x) > Mathf.Abs(normalizedDirection.y))
            {
                return normalizedDirection.x > 0 ? $"Attack{currentComboStep}East" : $"Attack{currentComboStep}West";
            }
            else
            {
                return normalizedDirection.y > 0 ? $"Attack{currentComboStep}North" : $"Attack{currentComboStep}South";
            }
        }

        private void PlayAttackAnimation(string animationName)
        {
            _animator.Play(animationName, 0, 0f);
            Debug.Log($"Animator playing: {animationName}");
        }

        private float GetAnimationLength(string animationName)
        {
            RuntimeAnimatorController ac = _animator.runtimeAnimatorController;
            foreach (AnimationClip clip in ac.animationClips)
            {
                if (clip.name == animationName)
                {
                    Debug.Log($"Retrieved animation length for {animationName}: {clip.length}s");
                    return clip.length;
                }
            }
            Debug.LogError($"Animation clip '{animationName}' not found in Animator.");
            return 0f;
        }

        public void QueueNextAttack(Vector2 newAttackDirection)
        {
            nextAttackQueued = true;
            SetAttackDirection(newAttackDirection);
            Debug.Log($"Next attack queued with new direction: {newAttackDirection}");
        }

        private void ApplyAttackMovement()
        {
            if (comboMovementSpeeds.Count >= currentComboStep)
            {
                float currentStepMovementSpeed = comboMovementSpeeds[currentComboStep - 1];
                _rigidbody.velocity = attackDirection * currentStepMovementSpeed;
                Debug.Log($"Applied attack movement for step {currentComboStep}: {_rigidbody.velocity}");
            }
        }
    }
}
