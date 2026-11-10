using System.Collections;
using UnityEngine;
using WickFall.Player;

namespace WickFall.Enemies
{
    public enum ThrallState
    {
        Patrol,
        Chase,
        Windup,
        Attack,
        Cooldown
    }

    /// <summary>
    /// AI Controller for the Shadow Thrall enemy.
    /// Wanders darkness, detects the player when inside its detection radius,
    /// chases with erratic top-down movement, and strikes with a telegraphed melee attack.
    /// Strictly complies with Unity 6 Physics2D linearVelocity.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class ShadowThrallController : MonoBehaviour
    {
        [Header("AI Movement")]
        [SerializeField] private float _patrolSpeed = 1.6f;
        [SerializeField] private float _chaseSpeed = 3.2f;
        [SerializeField] private float _detectionRadius = 5.5f;
        [SerializeField] private float _attackRange = 0.75f;
        [SerializeField] private float _patrolRadius = 3.0f;

        [Header("Combat")]
        [SerializeField] private float _attackDamage = 15f;
        [SerializeField] private float _windupDuration = 0.35f;
        [SerializeField] private float _attackCooldown = 1.0f;
        [SerializeField] private float _knockbackOnPlayer = 4.0f;

        [Header("Sprites")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite _idleSprite;
        [SerializeField] private Sprite[] _walkFrames;
        [SerializeField] private Sprite _attackSprite;
        [SerializeField] private float _walkFps = 6f;

        [Header("Audio")]
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioClip _screechSound;
        [SerializeField] private AudioClip _attackSound;

        private Rigidbody2D _rb;
        private Transform _playerTransform;
        private PlayerWax _playerWax;
        private Vector2 _spawnOrigin;
        private Vector2 _currentPatrolTarget;
        private ThrallState _state = ThrallState.Patrol;
        private float _patrolTimer;
        private float _animTimer;
        private int _walkFrameIndex;
        private bool _hasAlerted;

        public ThrallState CurrentState => _state;

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

            _spawnOrigin = transform.position;
            PickNewPatrolTarget();
        }

        private void Start()
        {
            FindPlayer();
        }

        private void FindPlayer()
        {
            var player = Object.FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                _playerTransform = player.transform;
                _playerWax = player.GetComponent<PlayerWax>();
            }
        }

        private void Update()
        {
            if (_playerTransform == null)
            {
                FindPlayer();
                if (_playerTransform == null) return;
            }

            if (_playerWax != null && _playerWax.IsDead)
            {
                _state = ThrallState.Patrol;
            }

            float distToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

            switch (_state)
            {
                case ThrallState.Patrol:
                    HandlePatrol(distToPlayer);
                    break;

                case ThrallState.Chase:
                    HandleChase(distToPlayer);
                    break;
            }

            UpdateVisuals();
        }

        private void HandlePatrol(float distToPlayer)
        {
            // Detect player
            if (distToPlayer <= _detectionRadius && (_playerWax == null || !_playerWax.IsDead))
            {
                _state = ThrallState.Chase;
                if (!_hasAlerted && _sfxSource != null && _screechSound != null)
                {
                    _hasAlerted = true;
                    _sfxSource.PlayOneShot(_screechSound);
                }
                return;
            }

            _patrolTimer += Time.deltaTime;
            float distToTarget = Vector2.Distance(transform.position, _currentPatrolTarget);

            if (distToTarget < 0.2f || _patrolTimer > 4.0f)
            {
                PickNewPatrolTarget();
                _patrolTimer = 0f;
            }

            Vector2 dir = (_currentPatrolTarget - (Vector2)transform.position).normalized;
            _rb.linearVelocity = dir * _patrolSpeed;
        }

        private void HandleChase(float distToPlayer)
        {
            // If player wandered out of chase range
            if (distToPlayer > _detectionRadius * 1.5f || (_playerWax != null && _playerWax.IsDead))
            {
                _state = ThrallState.Patrol;
                _hasAlerted = false;
                PickNewPatrolTarget();
                return;
            }

            // Close enough to strike
            if (distToPlayer <= _attackRange)
            {
                StartCoroutine(PerformAttackRoutine());
                return;
            }

            Vector2 dir = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
            _rb.linearVelocity = dir * _chaseSpeed;
        }

        private IEnumerator PerformAttackRoutine()
        {
            _state = ThrallState.Windup;
            Vector2 attackDir = _playerTransform != null 
                ? ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized 
                : Vector2.zero;

            // Telegraph windup: show attack claws & stalk slowly into striking range
            if (_attackSprite != null && _spriteRenderer != null)
            {
                _spriteRenderer.sprite = _attackSprite;
                _spriteRenderer.color = new Color(1.0f, 0.72f, 0.72f, 1f); // subtle aggressive red tint
            }
            _rb.linearVelocity = attackDir * (_chaseSpeed * 0.45f);

            yield return new WaitForSeconds(_windupDuration);

            _state = ThrallState.Attack;

            // Aggressive forward lunge directly into close melee contact
            _rb.linearVelocity = attackDir * (_chaseSpeed * 1.6f);

            if (_sfxSource != null && _attackSound != null)
            {
                _sfxSource.PlayOneShot(_attackSound);
            }

            // Check if player is struck in close melee reach
            if (_playerTransform != null && _playerWax != null && !_playerWax.IsDead)
            {
                float dist = Vector2.Distance(transform.position, _playerTransform.position);
                if (dist <= 0.95f)
                {
                    Vector2 knockbackDir = (_playerTransform.position - transform.position).normalized;
                    _playerWax.TakeDamage(_attackDamage, knockbackDir, _knockbackOnPlayer);
                }
            }

            yield return new WaitForSeconds(0.14f);
            _rb.linearVelocity = Vector2.zero;
            if (_spriteRenderer != null) _spriteRenderer.color = Color.white;

            _state = ThrallState.Cooldown;
            yield return new WaitForSeconds(_attackCooldown);

            _state = ThrallState.Chase;
        }

        private void PickNewPatrolTarget()
        {
            Vector2 randomOffset = Random.insideUnitCircle * _patrolRadius;
            _currentPatrolTarget = _spawnOrigin + randomOffset;
        }

        private void UpdateVisuals()
        {
            if (_spriteRenderer == null) return;

            // Flip facing
            if (_rb.linearVelocity.x > 0.05f)
            {
                _spriteRenderer.flipX = false;
            }
            else if (_rb.linearVelocity.x < -0.05f)
            {
                _spriteRenderer.flipX = true;
            }

            if (_state == ThrallState.Windup || _state == ThrallState.Attack)
            {
                return;
            }

            if (_rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                _animTimer += Time.deltaTime;
                float frameDuration = 1f / Mathf.Max(1f, _walkFps);
                if (_animTimer >= frameDuration)
                {
                    _animTimer -= frameDuration;
                    _walkFrameIndex = (_walkFrameIndex + 1) % (_walkFrames != null && _walkFrames.Length > 0 ? _walkFrames.Length : 1);
                }

                if (_walkFrames != null && _walkFrames.Length > 0)
                {
                    _spriteRenderer.sprite = _walkFrames[_walkFrameIndex];
                }
            }
            else
            {
                if (_idleSprite != null)
                {
                    _spriteRenderer.sprite = _idleSprite;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(Application.isPlaying ? _spawnOrigin : (Vector2)transform.position, _patrolRadius);
        }
    }
}
