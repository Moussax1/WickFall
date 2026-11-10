using System.Collections;
using UnityEngine;
using WickFall.Player;

namespace WickFall.Environment
{
    public enum SpikeState
    {
        Retracted,
        Warning,
        Extended
    }

    /// <summary>
    /// Lethal iron spikes embedded in stone tiles.
    /// Operates on a periodic timer: warning shake -> sudden snap extension dealing 25 wax damage.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SpikeTrap : MonoBehaviour
    {
        [Header("Cycle Timing")]
        [SerializeField] private float _retractedDuration = 2.4f;
        [SerializeField] private float _warningDuration = 0.6f;
        [SerializeField] private float _extendedDuration = 1.0f;

        [Header("Combat")]
        [SerializeField] private float _spikeDamage = 25f;
        [SerializeField] private float _knockbackForce = 5.0f;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _safeColor = new Color(0.4f, 0.4f, 0.4f, 0.4f);
        [SerializeField] private Color _warningColor = new Color(0.9f, 0.5f, 0.1f, 0.8f);
        [SerializeField] private Color _lethalColor = new Color(1.0f, 0.2f, 0.15f, 1.0f);

        [Header("Audio")]
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioClip _springSound;

        private SpikeState _state = SpikeState.Retracted;
        private readonly Collider2D[] _hitBuffer = new Collider2D[8];

        public SpikeState CurrentState => _state;

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;

            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_spriteRenderer != null)
            {
                _spriteRenderer.sortingOrder = -4; // Above floor, below actors
                _spriteRenderer.color = _safeColor;
            }
        }

        private void Start()
        {
            StartCoroutine(SpikeCycleRoutine());
        }

        private IEnumerator SpikeCycleRoutine()
        {
            // Stagger initial cycle based on position so traps don't all trigger at the exact same frame
            float initialOffset = Mathf.Abs(transform.position.x * 0.4f + transform.position.y * 0.6f) % 2.0f;
            yield return new WaitForSeconds(initialOffset);

            while (true)
            {
                // 1. Safe / Retracted
                _state = SpikeState.Retracted;
                if (_spriteRenderer != null) _spriteRenderer.color = _safeColor;
                yield return new WaitForSeconds(_retractedDuration);

                // 2. Warning shake & glow
                _state = SpikeState.Warning;
                float elapsed = 0f;
                Vector3 originalPos = transform.position;

                while (elapsed < _warningDuration)
                {
                    elapsed += Time.deltaTime;
                    float jitter = (Random.value - 0.5f) * 0.05f;
                    transform.position = originalPos + new Vector3(jitter, jitter, 0);
                    if (_spriteRenderer != null)
                    {
                        _spriteRenderer.color = Color.Lerp(_safeColor, _warningColor, elapsed / _warningDuration);
                    }
                    yield return null;
                }
                transform.position = originalPos;

                // 3. Extended / Lethal Snap!
                _state = SpikeState.Extended;
                if (_spriteRenderer != null) _spriteRenderer.color = _lethalColor;
                if (_sfxSource != null && _springSound != null)
                {
                    _sfxSource.PlayOneShot(_springSound);
                }

                // Check for victims standing on the trap
                DamageOccupants();

                yield return new WaitForSeconds(_extendedDuration);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_state == SpikeState.Extended)
            {
                TryDamage(other);
            }
        }

        private void DamageOccupants()
        {
            var col = GetComponent<Collider2D>();
            var bounds = col.bounds;
            int count = Physics2D.OverlapBoxNonAlloc(bounds.center, bounds.size, 0f, _hitBuffer);

            for (int i = 0; i < count; i++)
            {
                if (_hitBuffer[i] != null && _hitBuffer[i] != col)
                {
                    TryDamage(_hitBuffer[i]);
                }
            }
        }

        private void TryDamage(Collider2D target)
        {
            if (target.TryGetComponent<PlayerWax>(out var playerWax))
            {
                Vector2 knockback = (target.transform.position - transform.position).normalized;
                playerWax.TakeDamage(_spikeDamage, knockback, _knockbackForce);
            }
        }
    }
}
