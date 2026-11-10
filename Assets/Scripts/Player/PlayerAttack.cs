using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using WickFall.Enemies;

namespace WickFall.Player
{
    /// <summary>
    /// Executes the 4-directional Ember Strike melee attack.
    /// Handles forward lunge impulse, attack sprite sequence, hit detection with pre-allocated buffer,
    /// and knockback application on enemies.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAttack : MonoBehaviour
    {
        [Header("Combat Tuning")]
        [SerializeField] private float _damage = 25f;
        [SerializeField] private float _attackCooldown = 0.45f;
        [SerializeField] private float _attackOffset = 1.0f;
        [SerializeField] private float _hitRadius = 0.75f;
        [SerializeField] private float _lungeImpulse = 3.0f;
        [SerializeField] private float _knockbackForce = 5.0f;
        [SerializeField] private LayerMask _enemyLayer;

        [Header("Visuals & Sprites")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite[] _attackFrames;
        [SerializeField] private float _frameDuration = 0.06f;

        [Header("Audio")]
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioClip _swingClip;
        [SerializeField] private AudioClip _hitClip;

        private PlayerController _controller;
        private PlayerWax _wax;
        private float _lastAttackTime = -10f;
        private bool _isAttacking;
        private readonly Collider2D[] _hitBuffer = new Collider2D[16];

        public bool IsAttacking => _isAttacking;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _wax = GetComponent<PlayerWax>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void Update()
        {
            if (_wax != null && _wax.IsDead) return;
            if (_isAttacking) return;
            if (Time.time < _lastAttackTime + _attackCooldown) return;

            if (CheckAttackInput())
            {
                StartCoroutine(PerformAttackRoutine());
            }
        }

        private bool CheckAttackInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.jKey.wasPressedThisFrame))
            {
                return true;
            }

            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                return true;
            }

            var gamepad = Gamepad.current;
            if (gamepad != null && (gamepad.buttonWest.wasPressedThisFrame || gamepad.rightTrigger.wasPressedThisFrame))
            {
                return true;
            }

            return false;
        }

        private IEnumerator PerformAttackRoutine()
        {
            _isAttacking = true;
            _lastAttackTime = Time.time;
            _controller.SetAttacking(true);

            Vector2 facing = _controller.FacingVector;

            // Apply forward lunge
            _controller.ApplyImpulse(facing * _lungeImpulse);

            if (_sfxSource != null && _swingClip != null)
            {
                _sfxSource.PlayOneShot(_swingClip);
            }

            // Animate attack frames
            if (_attackFrames != null && _attackFrames.Length > 0 && _spriteRenderer != null)
            {
                for (int i = 0; i < _attackFrames.Length; i++)
                {
                    if (_attackFrames[i] != null)
                    {
                        _spriteRenderer.sprite = _attackFrames[i];
                    }

                    // Perform hitbox check on the active strike frame (frame 1 or 2)
                    if (i == 1)
                    {
                        ExecuteHitbox(facing);
                    }

                    yield return new WaitForSeconds(_frameDuration);
                }
            }
            else
            {
                ExecuteHitbox(facing);
                yield return new WaitForSeconds(0.2f);
            }

            _controller.SetAttacking(false);
            _isAttacking = false;
        }

        private void ExecuteHitbox(Vector2 facing)
        {
            Vector2 attackCenter = (Vector2)transform.position + facing * _attackOffset;
            int count = Physics2D.OverlapCircleNonAlloc(attackCenter, _hitRadius, _hitBuffer, _enemyLayer);

            bool hitAny = false;
            for (int i = 0; i < count; i++)
            {
                var col = _hitBuffer[i];
                if (col == null) continue;

                if (col.TryGetComponent<EnemyHealth>(out var enemyHealth))
                {
                    enemyHealth.TakeDamage(_damage, facing, _knockbackForce);
                    hitAny = true;
                }
            }

            if (hitAny && _sfxSource != null && _hitClip != null)
            {
                _sfxSource.PlayOneShot(_hitClip);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 facing = _controller != null ? _controller.FacingVector : Vector2.down;
            Vector2 attackCenter = (Vector2)transform.position + facing * _attackOffset;

            Gizmos.color = new Color(1f, 0.4f, 0f, 0.5f);
            Gizmos.DrawWireSphere(attackCenter, _hitRadius);
        }
    }
}
