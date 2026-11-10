using UnityEngine;
using WickFall.Player;

namespace WickFall.Enemies
{
    public enum ThrallState { Patrol, Chase, Windup, Attack, Cooldown }

    [RequireComponent(typeof(Rigidbody2D))]
    public class ShadowThrallController : MonoBehaviour
    {
        [SerializeField] private float _patrolSpeed = 1.6f;
        [SerializeField] private float _chaseSpeed = 3.0f;
        [SerializeField] private float _detectionRadius = 5.5f;
        [SerializeField] private float _attackRange = 0.8f;
        [SerializeField] private float _patrolRadius = 3.0f;
        [SerializeField] private float _attackDamage = 15f;
        [SerializeField] private float _windupDuration = 0.35f;
        [SerializeField] private float _attackCooldown = 1.0f;
        [SerializeField] private float _knockbackOnPlayer = 4.0f;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite _idleSprite;
        [SerializeField] private Sprite[] _walkFrames;
        [SerializeField] private Sprite _attackSprite;
        [SerializeField] private float _walkFps = 6f;

        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioClip _screechSound;
        [SerializeField] private AudioClip _attackSound;

        private Rigidbody2D _rb;
        private Transform _playerTransform;
        private PlayerWax _playerWax;
        private float _lastAttackTime = -10f;
        private ThrallState _state = ThrallState.Patrol;
        private Vector2 _targetVelocity = Vector2.zero;

        public ThrallState CurrentState => _state;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (_spriteRenderer != null && _idleSprite != null) _spriteRenderer.sprite = _idleSprite;
        }

        private void Start() => FindPlayer();

        private void FindPlayer()
        {
            var p = Object.FindFirstObjectByType<PlayerController>();
            if (p != null) { _playerTransform = p.transform; _playerWax = p.GetComponent<PlayerWax>(); }
        }

        private void Update()
        {
            if (_playerTransform == null) { FindPlayer(); if (_playerTransform == null) return; }
            if (_playerWax != null && _playerWax.IsDead) { _targetVelocity = Vector2.zero; return; }

            float dist = Vector2.Distance(transform.position, _playerTransform.position);

            if (dist <= _attackRange)
            {
                _targetVelocity = Vector2.zero;
                _state = ThrallState.Attack;
                if (_spriteRenderer != null && _attackSprite != null) _spriteRenderer.sprite = _attackSprite;

                if (Time.time >= _lastAttackTime + _attackCooldown)
                {
                    _lastAttackTime = Time.time;
                    Vector2 dir = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
                    if (_playerWax != null) _playerWax.TakeDamage(_attackDamage, dir, _knockbackOnPlayer);
                    if (_sfxSource != null && _attackSound != null) _sfxSource.PlayOneShot(_attackSound);
                }
            }
            else if (dist <= _detectionRadius)
            {
                _state = ThrallState.Chase;
                if (_spriteRenderer != null && _idleSprite != null) _spriteRenderer.sprite = _idleSprite;
                Vector2 dir = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
                _targetVelocity = dir * _chaseSpeed;
                if (_spriteRenderer != null) _spriteRenderer.flipX = dir.x < 0;
            }
            else
            {
                _state = ThrallState.Patrol;
                if (_spriteRenderer != null && _idleSprite != null) _spriteRenderer.sprite = _idleSprite;
                _targetVelocity = Vector2.zero;
            }
        }

        private void FixedUpdate()
        {
            if (_rb != null) _rb.linearVelocity = _targetVelocity;
        }
    }
}
