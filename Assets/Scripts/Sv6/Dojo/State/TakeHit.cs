using JetBrains.Annotations;
using UnityEngine;

namespace Sv6.Dojo.State
{
    public class TakeHit : Base
    {
        private Vector2 hitDirection;
        private bool isHit = false;
        private float hitDuration = 0.54f;
        private float hitTimer = 0f;

        // Reference to the CameraShake component
        private CameraShake cameraShake;

        public override void Enter()
        {
            base.Enter();
            isHit = true;
            hitTimer = 0f;
            _isComplete = false;

            // Play sound
            SoundManager.PlaySound("TakeHit");

            // CAMERA FREEZE
            CameraFreeze cameraFreeze = FindObjectOfType<CameraFreeze>();
            if (cameraFreeze != null)
            {
                cameraFreeze.Freeze();
            }
        
            // CAMERA SHAKE
            GameObject cameraShakeObject = GameObject.FindWithTag("CameraShake");
            if (cameraShakeObject != null)
            {
                cameraShake = cameraShakeObject.GetComponent<CameraShake>();
            }
            else
            {
                Debug.LogError("CameraShake object not found. Make sure your camera is tagged 'CameraShake' or update the tag.");
            }

            // Determine the hit direction
            hitDirection = _playerController.lastNonZeroDirection;
            if (hitDirection == Vector2.zero)
            {
                hitDirection = Vector2.right; // Default to right if idle
            }

            // Play the corresponding animation (the player's hit animation)
            string animationName = GetHitAnimationName(hitDirection);
            _animator.Play(animationName, 0, 0f);
            Debug.Log($"Playing hit animation: {animationName}");

            // Immediately trigger camera shake (so it only triggers once when entering the hit state)
            if (cameraShake != null)
            {
                cameraShake.Shake();
            }
        }

        public override void Do()
        {
            // Keep the player from moving while hit
            _rigidbody.velocity = Vector2.zero;
            
            // Update timer and check if the hit state should end
            hitTimer += Time.deltaTime;
            if (hitTimer >= hitDuration)
            {
                _isComplete = true;
            }
        }

        public override void Exit()
        {
            base.Exit();
            isHit = false;
        }

        /// <summary>
        /// Determines the player's hit animation based on direction.
        /// </summary>
        private string GetHitAnimationName(Vector2 direction)
        {
            Vector2 normalizedDirection = direction.normalized;
            float x = Mathf.Round(normalizedDirection.x);

            // Example logic: East or West damage animations
            if (Mathf.Abs(x) > 0)
            {
                return "DamageEast";
            }
            else
            {
                return "DamageWest";
            }
        }
    }
}
