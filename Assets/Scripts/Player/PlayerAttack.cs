using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using WickFall.Enemies;

namespace WickFall.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private float _damage = 25f;
        [SerializeField] private float _attackCooldown = 0.4f;
        [SerializeField] private float _attackOffset = 1.0f;
        [SerializeField] private float _hitRadius = 0.75f;
        [SerializeField] private float _lungeImpulse = 2.5f;
        [SerializeField] private float _knockbackForce = 5.0f;
        [SerializeField] private LayerMask _enemyLayer;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite[] _attackFrames;
        [SerializeField] private float _frameDuration = 0.06f;

        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioClip _swingClip;
        [SerializeField] private AudioClip _hitClip;

        private PlayerController _controller;
        private PlayerWax _wax;
        private float _lastAttackTime = -10f;
        private bool _isAttacking;

        public bool IsAttacking => _isAttacking;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _wax = GetComponent<PlayerWax>();
            if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Update()
        {
            if ((_wax != null && _wax.IsDead) || _isAttacking || Time.time < _lastAttackTime + _attackCooldown) return;

            var kb = Keyboard.current;
            var mouse = Mouse.current;
            var gp = Gamepad.current;
            bool attackPressed = (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.jKey.wasPressedThisFrame)) ||
                                 (mouse != null && mouse.leftButton.wasPressedThisFrame) ||
                                 (gp != null && gp.buttonWest.wasPressedThisFrame);

            if (attackPressed) StartCoroutine(AttackRoutine());
        }

        private IEnumerator AttackRoutine()
        {
            _isAttacking = true;
            _lastAttackTime = Time.time;
            _controller.SetAttacking(true);

            Vector2 facing = _controller.FacingVector;
            _controller.ApplyImpulse(facing * _lungeImpulse);
            if (_sfxSource != null && _swingClip != null) _sfxSource.PlayOneShot(_swingClip);

            // Hit detection
            Vector2 hitCenter = (Vector2)transform.position + facing * _attackOffset;
            Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, _hitRadius, _enemyLayer == 0 ? ~0 : _enemyLayer.value);
            foreach (var col in hits)
            {
                if (col.gameObject == gameObject) continue;
                if (col.TryGetComponent<EnemyHealth>(out var enemy))
                {
                    enemy.TakeDamage(_damage, facing, _knockbackForce);
                    if (_sfxSource != null && _hitClip != null) _sfxSource.PlayOneShot(_hitClip);
                }
            }

            // Animate frames
            if (_attackFrames != null)
            {
                for (int i = 0; i < _attackFrames.Length; i++)
                {
                    if (_attackFrames[i] != null && _spriteRenderer != null) _spriteRenderer.sprite = _attackFrames[i];
                    yield return new WaitForSeconds(_frameDuration);
                }
            }

            _controller.SetAttacking(false);
            _isAttacking = false;
        }
    }
}
