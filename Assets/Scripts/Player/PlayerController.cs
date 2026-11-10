using UnityEngine;
using UnityEngine.InputSystem;

namespace WickFall.Player
{
    /// <summary>
    /// Handles 4-directional top-down movement and sprite animation for the Candle Golem.
    /// Strictly complies with Unity 6 Input System and 2D Physics API (linearVelocity).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 4.2f;
        [SerializeField] private bool _canMove = true;

        [Header("Visual Components")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [Header("Directional Sprites - Idle")]
        [SerializeField] private Sprite _idleDown;
        [SerializeField] private Sprite _idleUp;
        [SerializeField] private Sprite _idleSide;

        [Header("Walk Animation Cycles (4 Frames Each)")]
        [SerializeField] private Sprite[] _walkDownFrames;
        [SerializeField] private Sprite[] _walkUpFrames;
        [SerializeField] private Sprite[] _walkSideFrames;
        [SerializeField] private float _animationFps = 8f;

        private Rigidbody2D _rb;
        private Vector2 _moveInput;
        private Vector2 _simulatedInput;
        private float _speedMultiplier = 1.0f;
        private PlayerFacingDirection _facingDirection = PlayerFacingDirection.South;
        private Vector2 _facingVector = Vector2.down;
        private float _animTimer;
        private int _currentFrameIndex;
        private bool _isAttacking;

        public PlayerFacingDirection FacingDirection => _facingDirection;
        public Vector2 FacingVector => _facingVector;
        public bool IsMoving => _canMove && _moveInput.sqrMagnitude > 0.01f;
        public Rigidbody2D Rb => _rb;

        public void SetSimulatedInput(Vector2 input)
        {
            _simulatedInput = input;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            UpdateFacingVector();
        }

        private void Update()
        {
            if (!_canMove)
            {
                _moveInput = Vector2.zero;
                return;
            }

            ReadInput();
            UpdateFacing();
            if (!_isAttacking)
            {
                UpdateAnimation(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (!_canMove || _isAttacking)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            _rb.linearVelocity = _moveInput.normalized * (_moveSpeed * _speedMultiplier);
        }

        private void ReadInput()
        {
            if (_simulatedInput.sqrMagnitude > 0.01f)
            {
                _moveInput = _simulatedInput;
                return;
            }

            Vector2 input = Vector2.zero;

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
            }

            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 stick = gamepad.leftStick.ReadValue();
                if (stick.sqrMagnitude > 0.05f)
                {
                    input = stick;
                }
                else
                {
                    Vector2 dpad = gamepad.dpad.ReadValue();
                    if (dpad.sqrMagnitude > 0.05f)
                    {
                        input = dpad;
                    }
                }
            }

            _moveInput = input.sqrMagnitude > 1f ? input.normalized : input;
        }

        private void UpdateFacing()
        {
            if (_moveInput.sqrMagnitude < 0.01f) return;

            // Prioritize the dominant axis for crisp 4-directional switching
            if (Mathf.Abs(_moveInput.x) > Mathf.Abs(_moveInput.y))
            {
                _facingDirection = _moveInput.x > 0 ? PlayerFacingDirection.East : PlayerFacingDirection.West;
            }
            else
            {
                _facingDirection = _moveInput.y > 0 ? PlayerFacingDirection.North : PlayerFacingDirection.South;
            }

            UpdateFacingVector();
        }

        private void UpdateFacingVector()
        {
            switch (_facingDirection)
            {
                case PlayerFacingDirection.North:
                    _facingVector = Vector2.up;
                    if (_spriteRenderer != null) _spriteRenderer.flipX = false;
                    break;
                case PlayerFacingDirection.South:
                    _facingVector = Vector2.down;
                    if (_spriteRenderer != null) _spriteRenderer.flipX = false;
                    break;
                case PlayerFacingDirection.East:
                    _facingVector = Vector2.right;
                    if (_spriteRenderer != null) _spriteRenderer.flipX = false;
                    break;
                case PlayerFacingDirection.West:
                    _facingVector = Vector2.left;
                    if (_spriteRenderer != null) _spriteRenderer.flipX = true;
                    break;
            }
        }

        private void UpdateAnimation(float deltaTime)
        {
            if (_spriteRenderer == null) return;

            if (!IsMoving)
            {
                _animTimer = 0f;
                _currentFrameIndex = 0;
                switch (_facingDirection)
                {
                    case PlayerFacingDirection.North:
                        if (_idleUp != null) _spriteRenderer.sprite = _idleUp;
                        break;
                    case PlayerFacingDirection.South:
                        if (_idleDown != null) _spriteRenderer.sprite = _idleDown;
                        break;
                    case PlayerFacingDirection.East:
                    case PlayerFacingDirection.West:
                        if (_idleSide != null) _spriteRenderer.sprite = _idleSide;
                        break;
                }
                return;
            }

            _animTimer += deltaTime;
            float frameDuration = 1f / Mathf.Max(1f, _animationFps);
            if (_animTimer >= frameDuration)
            {
                _animTimer -= frameDuration;
                _currentFrameIndex++;
            }

            Sprite[] currentCycle = _facingDirection switch
            {
                PlayerFacingDirection.North => _walkUpFrames,
                PlayerFacingDirection.South => _walkDownFrames,
                _ => _walkSideFrames
            };

            if (currentCycle != null && currentCycle.Length > 0)
            {
                int frame = _currentFrameIndex % currentCycle.Length;
                if (currentCycle[frame] != null)
                {
                    _spriteRenderer.sprite = currentCycle[frame];
                }
            }
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = Mathf.Clamp(multiplier, 0.1f, 2.5f);
        }

        public void SetCanMove(bool canMove)
        {
            _canMove = canMove;
            if (!_canMove && _rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }

        public void SetAttacking(bool isAttacking)
        {
            _isAttacking = isAttacking;
            if (_isAttacking && _rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }

        public void ApplyImpulse(Vector2 impulse)
        {
            if (_rb != null)
            {
                _rb.AddForce(impulse, ForceMode2D.Impulse);
            }
        }
    }
}
