using UnityEngine;
using System.Collections;

namespace Sv6.Dojo.State
{
    public class Die : Base
    {
        private PlayerController _playerController;

        public override void Setup(PlayerController playerController, Rigidbody2D rigidbody, Animator animator, float moveSpeed)
        {
            base.Setup(playerController, rigidbody, animator, moveSpeed);
            _playerController = playerController;
        }

        public override void Enter()
        {
            base.Enter();

            // Stop all player movements
            _rigidbody.velocity = Vector2.zero;

            // Prevent further inputs
            _playerController.isActive = false;

            // Play sound for Die animation
            SoundManager.PlaySound("GastonDie");

            // Determine which Die animation to play
            string dieAnimation = _playerController.lastNonZeroDirection.x > 0 ? "DieEast" : "DieWest";

            // Play the chosen Die animation
            _animator.Play(dieAnimation);
            _playerController.currentAnimation = dieAnimation;

            // Start the death sequence coroutine
            _playerController.StartCoroutine(HandleDeathSequence(dieAnimation));
        }

        public override void Do()
        {
            // The Die state does not allow further actions
        }

        public override void Exit()
        {
            // Allow player input if transitioning from Die state (unlikely but good practice)
            _playerController.isActive = true;
        }

        private IEnumerator HandleDeathSequence(string dieAnimation)
        {
            // Wait until the Die animation starts playing
            while (!_animator.GetCurrentAnimatorStateInfo(0).IsName(dieAnimation))
            {
                yield return null;
            }

            // Wait for the Die animation to finish
            while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }

            // Remove HealthBar child object
            Transform healthBarTransform = _playerController.transform.Find("HealthBar");
            if (healthBarTransform != null)
            {
                Destroy(healthBarTransform.gameObject);
                Debug.Log("HealthBar removed from player.");
            }

            // Transition to the corresponding Dead animation
            string deadAnimation = _playerController.lastNonZeroDirection.x > 0 ? "DeadEast" : "DeadWest";
            _animator.Play(deadAnimation);
            _playerController.currentAnimation = deadAnimation;

            // Freeze the Rigidbody to simulate a static death state
            _rigidbody.simulated = false;

            Debug.Log("Player is now dead. Static Dead animation playing.");
        }
    }
}
