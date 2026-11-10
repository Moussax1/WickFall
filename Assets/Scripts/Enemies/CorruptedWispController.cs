using System.Collections;
using UnityEngine;
using WickFall.Player;
using WickFall.Items;

namespace WickFall.Enemies
{
    /// <summary>
    /// Elite ethereal specter that phases through dungeon walls.
    /// Hovers hauntingly outside melee range and channels a spectral Wax Siphon beam,
    /// draining the Golem's wax and violently destabilizing the candle flame.
    /// When defeated, dispels into a burst of collectible wax sparks.
    /// Complies strictly with Unity 6 Physics2D and Rigidbody2D.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class CorruptedWispController : MonoBehaviour
    {
        [Header("Patrol & Stalking")]
        [SerializeField] private Vector2 _origin;
        [SerializeField] private float _hoverSpeed = 2.0f;
        [SerializeField] private float _swoopSpeed = 3.8f;
        [SerializeField] private float _detectionRadius = 7.5f;
        [SerializeField] private float _preferredDistance = 2.8f;
        [SerializeField] private float _bobFrequency = 4.5f;
        [SerializeField] private float _bobAmplitude = 0.16f;

        [Header("Wax Siphon Mechanic")]
        [SerializeField] private float _siphonRange = 4.2f;
        [SerializeField] private float _siphonRate = 4.5f; // Wax drained per second
        [SerializeField] private LineRenderer _siphonTether;
        [SerializeField] private Color _tetherStartColor = new Color(0.4f, 0.9f, 1.0f, 0.85f);
        [SerializeField] private Color _tetherEndColor = new Color(0.9f, 0.4f, 1.0f, 0.5f);

        [Header("Death Spark Reward")]
        [SerializeField] private GameObject _waxSparkPrefab;
        [SerializeField] private int _sparkBurstCount = 3;

        [Header("Visuals & Audio")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioClip _screechSound;

        private Rigidbody2D _rb;
        private Transform _playerTransform;
        private PlayerWax _playerWax;
        private CandleLight _playerCandleLight;
        private EnemyHealth _health;
        private bool _isSiphoning;
        private float _orbitAngle;

        public Vector2 Origin => _origin;

        public void SetOrigin(Vector2 origin)
        {
            _origin = origin;
            transform.position = new Vector3(origin.x, origin.y, 0f);
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;

            var col = GetComponent<Collider2D>();
            col.isTrigger = true; // Phases through walls

            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            _health = GetComponent<EnemyHealth>();
            SetupLineRenderer();
        }

        private void Start()
        {
            // Lock origin to initial placed position if not assigned
            if (_origin == Vector2.zero)
            {
                _origin = transform.position;
            }
            else
            {
                transform.position = new Vector3(_origin.x, _origin.y, 0f);
            }

            if (_health != null)
            {
                _health.OnDied += HandleDeathBurst;
            }

            FindPlayer();
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnDied -= HandleDeathBurst;
            }
        }

        private void SetupLineRenderer()
        {
            if (_siphonTether == null)
            {
                _siphonTether = gameObject.GetComponent<LineRenderer>();
                if (_siphonTether == null)
                {
                    _siphonTether = gameObject.AddComponent<LineRenderer>();
                }
            }

            _siphonTether.useWorldSpace = true;
            _siphonTether.positionCount = 2;
            _siphonTether.startWidth = 0.12f;
            _siphonTether.endWidth = 0.04f;
            _siphonTether.startColor = _tetherStartColor;
            _siphonTether.endColor = _tetherEndColor;
            _siphonTether.sortingOrder = 4; // Above floor, below player
            _siphonTether.enabled = false;

            // Use lightweight Sprite-Default unlit material
            var defaultMat = new Material(Shader.Find("Sprites/Default"));
            _siphonTether.material = defaultMat;
        }

        private void FindPlayer()
        {
            var player = Object.FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                _playerTransform = player.transform;
                _playerWax = player.GetComponent<PlayerWax>();
                _playerCandleLight = player.GetComponentInChildren<CandleLight>();
            }
        }

        private void Update()
        {
            if (_playerTransform == null)
            {
                FindPlayer();
                if (_playerTransform == null) return;
            }

            float distToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
            bool playerAlive = _playerWax != null && !_playerWax.IsDead;

            if (distToPlayer <= _detectionRadius && playerAlive)
            {
                UpdateStalkAndSiphon(distToPlayer);
            }
            else
            {
                UpdatePatrol();
            }

            UpdateBobbing();
        }

        private void UpdateStalkAndSiphon(float dist)
        {
            Vector2 toPlayer = (Vector2)_playerTransform.position - (Vector2)transform.position;
            Vector2 dir = toPlayer.normalized;

            // Maintain standoff distance around preferred distance
            Vector2 desiredVel = Vector2.zero;
            if (dist > _preferredDistance + 0.5f)
            {
                // Advance toward player
                desiredVel = dir * _swoopSpeed;
            }
            else if (dist < _preferredDistance - 0.4f)
            {
                // Back away from Golem's melee range
                desiredVel = -dir * (_swoopSpeed * 0.8f);
            }
            else
            {
                // Orbit laterally around the player
                _orbitAngle += Time.deltaTime * 1.5f;
                Vector2 tangent = new Vector2(-dir.y, dir.x);
                desiredVel = tangent * (_hoverSpeed * 1.4f);
            }

            _rb.linearVelocity = desiredVel;

            // Siphon ray channeling
            if (dist <= _siphonRange)
            {
                if (!_isSiphoning)
                {
                    _isSiphoning = true;
                    if (_sfxSource != null && _screechSound != null)
                    {
                        _sfxSource.PlayOneShot(_screechSound, 0.6f);
                    }
                }

                // Tether from wisp center to candle wick
                Vector3 wickPos = _playerTransform.position + new Vector3(0f, 0.35f, 0f);
                _siphonTether.enabled = true;
                _siphonTether.SetPosition(0, transform.position);
                _siphonTether.SetPosition(1, wickPos);

                // Drain wax continuously (via DrainWax so it does NOT trigger melee invulnerability frames)
                if (_playerWax != null)
                {
                    _playerWax.DrainWax(_siphonRate * Time.deltaTime);
                }

                // Destabilize candle light flame
                if (_playerCandleLight != null)
                {
                    _playerCandleLight.SetFlameDistortion(2.8f);
                }

                // Pulse wisp color with spectral siphon glow
                if (_spriteRenderer != null)
                {
                    float pulse = (Mathf.Sin(Time.time * 10f) + 1f) * 0.5f;
                    _spriteRenderer.color = Color.Lerp(Color.white, new Color(0.5f, 0.9f, 1f, 1f), pulse);
                }
            }
            else
            {
                BreakSiphon();
            }
        }

        private void BreakSiphon()
        {
            _isSiphoning = false;
            if (_siphonTether != null)
            {
                _siphonTether.enabled = false;
            }

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.white;
            }
        }

        private void UpdatePatrol()
        {
            BreakSiphon();

            // Gentle floating patrol around assigned dungeon origin
            float angle = Time.time * _hoverSpeed;
            float bob = Mathf.Sin(Time.time * _bobFrequency) * _bobAmplitude;
            Vector2 target = _origin + new Vector2(Mathf.Cos(angle) * 2.0f, Mathf.Sin(angle * 0.7f) * 1.5f + bob);
            Vector2 dir = (target - (Vector2)transform.position);

            if (dir.magnitude > 0.1f)
            {
                _rb.linearVelocity = dir.normalized * _hoverSpeed;
            }
            else
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }

        private void UpdateBobbing()
        {
            float bob = Mathf.Sin(Time.time * _bobFrequency) * _bobAmplitude;
            if (_spriteRenderer != null)
            {
                // Only offset localPosition if the SpriteRenderer is on a dedicated child GameObject
                if (_spriteRenderer.transform != transform)
                {
                    _spriteRenderer.transform.localPosition = new Vector3(0f, bob, 0f);
                }

                if (_rb.linearVelocity.x > 0.05f) _spriteRenderer.flipX = false;
                else if (_rb.linearVelocity.x < -0.05f) _spriteRenderer.flipX = true;
            }
        }

        private void HandleDeathBurst()
        {
            BreakSiphon();

            // Spawn radial burst of 3 wax pickups
            if (_waxSparkPrefab != null)
            {
                for (int i = 0; i < _sparkBurstCount; i++)
                {
                    float angle = (i * (360f / _sparkBurstCount)) * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3(Mathf.Cos(angle) * 0.6f, Mathf.Sin(angle) * 0.6f, 0f);
                    Instantiate(_waxSparkPrefab, transform.position + offset, Quaternion.identity);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _siphonRange);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(new Vector3(_origin.x, _origin.y, 0f), 0.5f);
        }
    }
}
