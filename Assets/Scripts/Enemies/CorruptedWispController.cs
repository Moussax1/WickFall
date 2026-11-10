using System.Collections;
using UnityEngine;
using WickFall.Player;

namespace WickFall.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(LineRenderer))]
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

        [Header("Wax Siphon")]
        [SerializeField] private float _siphonRange = 4.2f;
        [SerializeField] private float _siphonRate = 4.5f;
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
        private Vector2 _targetVelocity = Vector2.zero;

        public Vector2 Origin => _origin;
        public void SetOrigin(Vector2 origin) { _origin = origin; transform.position = (Vector3)origin; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;

            var col = GetComponent<Collider2D>();
            col.isTrigger = true; // Phases through catacomb walls

            if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _health = GetComponent<EnemyHealth>();

            SetupTether();
        }

        private void SetupTether()
        {
            _siphonTether = GetComponent<LineRenderer>();
            if (_siphonTether == null)
            {
                _siphonTether = gameObject.AddComponent<LineRenderer>();
            }

            if (_siphonTether != null)
            {
                _siphonTether.useWorldSpace = true;
                _siphonTether.positionCount = 2;
                _siphonTether.startWidth = 0.12f;
                _siphonTether.endWidth = 0.04f;
                _siphonTether.startColor = _tetherStartColor;
                _siphonTether.endColor = _tetherEndColor;
                _siphonTether.sortingOrder = 4;
                _siphonTether.enabled = false;

                var shader = Shader.Find("Sprites/Default");
                if (shader != null) _siphonTether.material = new Material(shader);
            }
        }

        private void Start()
        {
            if (_origin == Vector2.zero) _origin = transform.position;
            if (_health != null) _health.OnDied += HandleDeathBurst;
            FindPlayer();
        }

        private void OnDestroy()
        {
            if (_health != null) _health.OnDied -= HandleDeathBurst;
            if (_siphonTether != null && _siphonTether.material != null)
            {
                Destroy(_siphonTether.material);
            }
        }

        private void FindPlayer()
        {
            var p = Object.FindFirstObjectByType<PlayerController>();
            if (p != null)
            {
                _playerTransform = p.transform;
                _playerWax = p.GetComponent<PlayerWax>();
                _playerCandleLight = p.GetComponentInChildren<CandleLight>();
            }
        }

        private void Update()
        {
            if (_playerTransform == null) { FindPlayer(); if (_playerTransform == null) return; }

            if (_playerWax != null && _playerWax.IsDead)
            {
                BreakSiphon();
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            float dist = Vector2.Distance(transform.position, _playerTransform.position);
            Vector2 toPlayer = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;

            if (dist <= _detectionRadius)
            {
                // Stalking behavior: maintain preferred distance & laterally orbit
                Vector2 desiredVel;
                if (dist > _preferredDistance + 0.5f)
                {
                    desiredVel = toPlayer * _swoopSpeed;
                }
                else if (dist < _preferredDistance - 0.4f)
                {
                    desiredVel = -toPlayer * (_swoopSpeed * 0.8f);
                }
                else
                {
                    Vector2 tangent = new Vector2(-toPlayer.y, toPlayer.x);
                    desiredVel = tangent * (_hoverSpeed * 1.4f);
                }

                _targetVelocity = desiredVel;

                // Siphon channeling
                if (dist <= _siphonRange)
                {
                    if (!_isSiphoning)
                    {
                        _isSiphoning = true;
                        if (_sfxSource != null && _screechSound != null) _sfxSource.PlayOneShot(_screechSound, 0.6f);
                    }

                    if (_siphonTether != null)
                    {
                        Vector3 wickPos = _playerTransform.position + new Vector3(0f, 0.35f, 0f);
                        _siphonTether.enabled = true;
                        _siphonTether.SetPosition(0, transform.position);
                        _siphonTether.SetPosition(1, wickPos);
                    }

                    if (_playerWax != null) _playerWax.DrainWax(_siphonRate * Time.deltaTime);
                    if (_playerCandleLight != null) _playerCandleLight.SetFlameDistortion(2.8f);

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
            else
            {
                BreakSiphon();
                // Gentle floating patrol around origin
                float angle = Time.time * _hoverSpeed;
                float bob = Mathf.Sin(Time.time * _bobFrequency) * _bobAmplitude;
                Vector2 target = _origin + new Vector2(Mathf.Cos(angle) * 2f, Mathf.Sin(angle * 0.7f) * 1.5f + bob);
                Vector2 dir = target - (Vector2)transform.position;
                _targetVelocity = dir.magnitude > 0.1f ? dir.normalized * _hoverSpeed : Vector2.zero;
            }

            if (_spriteRenderer != null)
            {
                if (_targetVelocity.x > 0.05f) _spriteRenderer.flipX = false;
                else if (_targetVelocity.x < -0.05f) _spriteRenderer.flipX = true;
            }
        }

        private void FixedUpdate()
        {
            if (_rb != null) _rb.linearVelocity = _targetVelocity;
        }

        private void BreakSiphon()
        {
            _isSiphoning = false;
            if (_siphonTether != null) _siphonTether.enabled = false;
            if (_spriteRenderer != null) _spriteRenderer.color = Color.white;
        }

        private void HandleDeathBurst()
        {
            BreakSiphon();

            if (_waxSparkPrefab != null && _sparkBurstCount > 0)
            {
                for (int i = 0; i < _sparkBurstCount; i++)
                {
                    float angle = (i * (360f / _sparkBurstCount)) * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3(Mathf.Cos(angle) * 0.6f, Mathf.Sin(angle) * 0.6f, 0f);
                    Instantiate(_waxSparkPrefab, transform.position + offset, Quaternion.identity);
                }
            }
        }
    }
}
