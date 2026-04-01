using UnityEngine;

namespace Sv6.Dojo.State
{
    public class Dash : Base
    {
        [SerializeField] private float _dashDistance = 1f;
        [SerializeField] private float _dashSpeed = 2f;
        private Vector2 _dashDirection;
        private Vector2 _dashStartPosition;
        private float dashTimer;  // Timer to track dash duration
        private const float dashDurationLimit = 0.35f; // Fail-safe duration limit

        public override void Enter()
        {
            base.Enter();

            // Calculate dash direction based on mouse cursor position
            Vector2 playerPos = _playerController.transform.position;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _dashDirection = (mousePos - playerPos).normalized;

            // Set dash start position
            _dashStartPosition = playerPos;

            // Set the Rigidbody2D velocity in the dash direction
            _rigidbody.velocity = _dashDirection * _dashSpeed;

            // Play the appropriate dash animation
            string dashAnimationName = GetDashAnimationName(_dashDirection);
            _animator.Play(dashAnimationName);

            // Reset dash timer
            dashTimer = 0f;
        }

        public override void Do()
        {
            // Increment dash timer
            dashTimer += Time.deltaTime;

            // Apply a force to counteract drag and maintain dash speed
            Vector2 counterDragForce = _dashDirection * (_dashSpeed / _rigidbody.drag);
            _rigidbody.AddForce(counterDragForce, ForceMode2D.Force);

            // Check if dash distance is reached or timer exceeds the limit
            if (Vector2.Distance(_playerController.transform.position, _dashStartPosition) >= _dashDistance || dashTimer >= dashDurationLimit)
            {
                // Stop the dash
                _rigidbody.velocity = Vector2.zero;
                _isComplete = true;
            }
        }

        public override void Exit()
        {
            base.Exit();
            // Stop any residual movement
            _rigidbody.velocity = Vector2.zero;
        }

        private string GetDashAnimationName(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                return direction.x > 0 ? "DashEast" : "DashWest";
            }
            else
            {
                return direction.y > 0 ? "DashNorth" : "DashSouth";
            }
        }
    }
}
