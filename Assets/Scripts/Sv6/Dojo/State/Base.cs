using Sv6.Dojo;
using UnityEngine;

namespace Sv6.Dojo.State
{
    public abstract class Base : MonoBehaviour
    {
        public bool _isComplete { get; protected set; }
        protected float _xInput;
        protected float _yInput;
        protected float _moveSpeed;
        protected PlayerController _playerController;
        protected Rigidbody2D _rigidbody;
        protected Animator _animator;

        public virtual void Setup(PlayerController playerController, Rigidbody2D rigidbody, Animator animator, float moveSpeed)
        {
            _playerController = playerController;
            _rigidbody = rigidbody;
            _animator = animator;
            _moveSpeed = moveSpeed;
        }

        public void SetInput(float xInput, float yInput)
        {
            _xInput = xInput;
            _yInput = yInput;
        }

        public virtual void Enter()
        {
            _isComplete = false;
        }

        public virtual void Do()
        {
            CheckStateCompletion();
        }

        public virtual void Exit()
        {

        }

        protected virtual void CheckStateCompletion()
        {

        }
    }
}
