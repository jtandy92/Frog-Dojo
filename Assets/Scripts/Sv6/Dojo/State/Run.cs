using UnityEngine;
using UnityEngine.InputSystem;

namespace Sv6.Dojo.State
{
    public class Run : Base
    {
        private string _currentAnimation = "";

        public override void Enter()
        {
            base.Enter();
            _currentAnimation = "";
        }

        public override void Do()
        {
            if (_playerController.direction == Vector2.zero ||
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                _rigidbody.velocity = Vector2.zero;
                _isComplete = true;
            }
            else
            {
                Vector2 newDirection = _playerController.direction.normalized;
                _playerController.lastNonZeroDirection = newDirection;
                _rigidbody.velocity = newDirection * _moveSpeed;

                string animationName = GetRunAnimationName(newDirection);

                if (_currentAnimation != animationName)
                {
                    _animator.Play(animationName);
                    _currentAnimation = animationName;
                }
            }
        }

        public override void Exit()
        {
            _currentAnimation = "";
        }

        private string GetRunAnimationName(Vector2 direction)
        {
            string animationName = "";
            Vector2 normalizedDirection = direction.normalized;
            float x = Mathf.Round(normalizedDirection.x);
            float y = Mathf.Round(normalizedDirection.y);

            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                animationName = x > 0 ? "RunEast" : "RunWest";
            }
            else
            {
                animationName = y > 0 ? "RunNorth" : "RunSouth";
            }

            return animationName;
        }
    }
}
