using UnityEngine;
using UnityEngine.InputSystem;

namespace WickFall.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 4.2f;
        [SerializeField] private bool _canMove = true;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite _idleDown;
        [SerializeField] private Sprite _idleUp;
        [SerializeField] private Sprite _idleSide;
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
        private int _frameIndex;
        private bool _isAttacking;

        public PlayerFacingDirection FacingDirection => _facingDirection;
        public Vector2 FacingVector => _facingVector;
        public bool IsMoving => _canMove && _moveInput.sqrMagnitude > 0.01f;
        public Rigidbody2D Rb => _rb;

        public void SetSimulatedInput(Vector2 input) => _simulatedInput = input;
        public void SetSpeedMultiplier(float m) => _speedMultiplier = Mathf.Clamp(m, 0.1f, 2.5f);
        public void SetCanMove(bool canMove) { _canMove = canMove; if (!_canMove && _rb != null) _rb.linearVelocity = Vector2.zero; }
        public void SetAttacking(bool attacking) { _isAttacking = attacking; if (_isAttacking && _rb != null) _rb.linearVelocity = Vector2.zero; }
        public void ApplyImpulse(Vector2 impulse) { if (_rb != null) _rb.AddForce(impulse, ForceMode2D.Impulse); }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Update()
        {
            if (!_canMove || _isAttacking) { _moveInput = Vector2.zero; return; }

            ReadInput();
            UpdateFacing();
            UpdateAnimation(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (!_canMove) { _rb.linearVelocity = Vector2.zero; return; }
            if (_isAttacking) return; // Retain impulse velocity to allow attack lunges to break out/escape!
            _rb.linearVelocity = _moveInput * (_moveSpeed * _speedMultiplier);
        }

        private void ReadInput()
        {
            if (_simulatedInput.sqrMagnitude > 0.01f) { _moveInput = _simulatedInput; return; }

            Vector2 input = Vector2.zero;
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) input.y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) input.y -= 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input.x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;
            }
            var gp = Gamepad.current;
            if (gp != null && input == Vector2.zero)
            {
                Vector2 stick = gp.leftStick.ReadValue();
                input = stick.sqrMagnitude > 0.05f ? stick : gp.dpad.ReadValue();
            }

            _moveInput = input.normalized;
        }

        private void UpdateFacing()
        {
            if (_moveInput.sqrMagnitude < 0.01f) return;

            if (Mathf.Abs(_moveInput.x) > Mathf.Abs(_moveInput.y))
            {
                _facingDirection = _moveInput.x > 0 ? PlayerFacingDirection.East : PlayerFacingDirection.West;
                _facingVector = _moveInput.x > 0 ? Vector2.right : Vector2.left;
                if (_spriteRenderer != null) _spriteRenderer.flipX = _moveInput.x < 0;
            }
            else
            {
                _facingDirection = _moveInput.y > 0 ? PlayerFacingDirection.North : PlayerFacingDirection.South;
                _facingVector = _moveInput.y > 0 ? Vector2.up : Vector2.down;
                if (_spriteRenderer != null) _spriteRenderer.flipX = false;
            }
        }

        private void UpdateAnimation(float dt)
        {
            if (_spriteRenderer == null) return;
            if (!IsMoving)
            {
                _spriteRenderer.sprite = _facingDirection switch
                {
                    PlayerFacingDirection.North => _idleUp,
                    PlayerFacingDirection.South => _idleDown,
                    _ => _idleSide
                };
                return;
            }

            _animTimer += dt;
            if (_animTimer >= 1f / Mathf.Max(1f, _animationFps))
            {
                _animTimer = 0f;
                _frameIndex++;
            }

            Sprite[] cycle = _facingDirection switch
            {
                PlayerFacingDirection.North => _walkUpFrames,
                PlayerFacingDirection.South => _walkDownFrames,
                _ => _walkSideFrames
            };

            if (cycle != null && cycle.Length > 0)
                _spriteRenderer.sprite = cycle[_frameIndex % cycle.Length];
        }
    }
}
