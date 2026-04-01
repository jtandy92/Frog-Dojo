using UnityEngine;
using UnityEngine.InputSystem;

namespace Sv6.Dojo
{
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 1f;
        public float invulnerabilityDuration = 1f;

        private bool _isInvulnerable = false;
        private float _invulnerabilityTimer = 0f;

        private State.Run _runningState;
        private State.Idle _idleState;
        private State.Attack _attackState;
        private State.Dash _dashState;
        private State.TakeHit _takeHitState;
        private State.Die _dieState;

        private State.Base _currentState;
        public Animator animator;
        public Rigidbody2D body;
        public string currentAnimation;

        [SerializeField] private InputActionAsset inputActions;
        private InputActionMap playerMap;
        private InputAction moveAction;
        private InputAction attackAction;
        private InputAction dashAction;

        public Vector2 direction = Vector2.zero;
        public Vector2 lastNonZeroDirection = Vector2.down;

        public bool isActive = true;

        private Health _health;
        private Camera _mainCamera;

        private void Awake()
        {
            InitializeInputActions();
            _mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            EnableAllActions();
        }

        private void OnDisable()
        {
            DisableAllActions();
        }

        void Start()
        {
            animator = GetComponent<Animator>();
            body = GetComponent<Rigidbody2D>();

            // Grab references to all child state scripts
            _runningState = gameObject.GetComponentInChildren<State.Run>();
            _idleState = gameObject.GetComponentInChildren<State.Idle>();
            _attackState = gameObject.GetComponentInChildren<State.Attack>();
            _dashState = gameObject.GetComponentInChildren<State.Dash>();
            _takeHitState = gameObject.GetComponentInChildren<State.TakeHit>();
            _dieState = gameObject.GetComponentInChildren<State.Die>();

            // Initialize them
            _runningState.Setup(this, body, animator, moveSpeed);
            _idleState.Setup(this, body, animator, moveSpeed);
            _attackState.Setup(this, body, animator, moveSpeed);
            _dashState.Setup(this, body, animator, moveSpeed);
            _takeHitState.Setup(this, body, animator, moveSpeed);
            _dieState.Setup(this, body, animator, moveSpeed);

            // Start in Idle
            _currentState = _idleState;
            _currentState.Enter();

            // If there's a Health component, subscribe to death events
            _health = GetComponent<Health>();
            if (_health != null)
            {
                _health.OnPlayerDeath += HandleDeath;
            }
        }

        void Update()
        {
            if (!isActive) return;

            // Handle brief invulnerability
            if (_isInvulnerable)
            {
                _invulnerabilityTimer += Time.deltaTime;
                if (_invulnerabilityTimer >= invulnerabilityDuration)
                {
                    _isInvulnerable = false;
                    _invulnerabilityTimer = 0f;
                }
            }

            // Update current state
            _currentState.SetInput(direction.x, direction.y);
            _currentState.Do();

            // If the current state is complete, pick the next one (Run or Idle)
            if (_currentState._isComplete)
            {
                SelectState();
            }
        }

        void InitializeInputActions()
        {
            playerMap = inputActions.FindActionMap("Player");
            moveAction = playerMap.FindAction("Move");
            attackAction = playerMap.FindAction("Attack");
            dashAction = playerMap.FindAction("Dash");

            moveAction.performed += OnMovePerformed;
            moveAction.canceled += OnMoveCanceled;
            attackAction.performed += OnAttackPerformed;
            dashAction.performed += OnDashPerformed;
        }

        void EnableAllActions()
        {
            playerMap.Enable();
        }

        void DisableAllActions()
        {
            playerMap.Disable();
        }

        void OnMovePerformed(InputAction.CallbackContext context)
        {
            if (!isActive) return;
            direction = context.ReadValue<Vector2>();
            if (direction != Vector2.zero)
            {
                lastNonZeroDirection = direction.normalized;
            }
        }

        void OnMoveCanceled(InputAction.CallbackContext context)
        {
            if (!isActive) return;
            direction = Vector2.zero;
        }

        void OnAttackPerformed(InputAction.CallbackContext context)
        {
            if (!isActive) return;

            // Calculate mouse direction from player to mouse
            Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 attackDirection = (mouseWorldPosition - transform.position).normalized;

            // If already in Attack, queue up a combo
            if (_currentState is State.Attack attackState)
            {
                attackState.QueueNextAttack(attackDirection);
            }
            // Else, transition into Attack if Idle/Run or the state just ended
            else if (_currentState._isComplete || _currentState is State.Idle || _currentState is State.Run)
            {
                Debug.Log($"Transitioning to Attack State. Attack Direction: {attackDirection}");
                _attackState.SetAttackDirection(attackDirection);
                TransitionToState(_attackState);
            }
        }

        /// <summary>
        /// Dashes at any moment, interrupting the current state (including Attack).
        /// Remove or refine conditions if you want to block dash while dying, etc.
        /// </summary>
        void OnDashPerformed(InputAction.CallbackContext context)
        {
            if (!isActive) return;

            // Example: if you want to prevent dashing while dying:
            // if (_currentState is State.Die || _currentState is State.TakeHit) return;

            // Simply transition to dash, which calls _currentState.Exit() on Attack, 
            // effectively canceling the animation.
            TransitionToState(_dashState);
        }

        public void HandleHit(int damageAmount)
        {
            if (!isActive || _isInvulnerable) return;

            _isInvulnerable = true;
            TransitionToState(_takeHitState);
        }

        void SelectState()
        {
            if (direction == Vector2.zero)
            {
                TransitionToState(_idleState);
            }
            else
            {
                TransitionToState(_runningState);
            }
        }

        void TransitionToState(State.Base newState)
        {
            if (_currentState != newState)
            {
                _currentState.Exit();
                _currentState = newState;
                _currentState.Enter();
            }
        }

        private void OnDestroy()
        {
            // Cleanup input subscriptions
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
            attackAction.performed -= OnAttackPerformed;
            dashAction.performed -= OnDashPerformed;

            // Cleanup Health subscriptions
            if (_health != null)
            {
                _health.OnPlayerDeath -= HandleDeath;
            }
        }

        public void HandleDeath()
        {
            TransitionToState(_dieState);

            FindObjectOfType<GameOverManager>()?.TriggerGameOver();
        }
    }
}
