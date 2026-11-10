using System;
using System.Collections;
using UnityEngine;
using WickFall.Items;

namespace WickFall.Enemies
{
    /// <summary>
    /// Tracks enemy health pool, hit-flash feedback, knockback response, and defeat rewards.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float _maxHealth = 50f;
        [SerializeField] private float _currentHealth = 50f;

        [Header("Drop Rewards")]
        [SerializeField] private GameObject _waxShardPrefab;
        [Range(0f, 1f)]
        [SerializeField] private float _dropChance = 0.65f;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _flashColor = new Color(1f, 0.4f, 0.4f, 1f);

        [Header("Audio")]
        [SerializeField] private AudioClip _deathSound;

        private Rigidbody2D _rb;
        private Color _originalColor = Color.white;
        private bool _isDead;
        private Coroutine _flashRoutine;

        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public bool IsDead => _isDead;

        public event Action<float, float> OnHealthChanged;
        public event Action OnDied;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (_spriteRenderer != null)
            {
                _originalColor = _spriteRenderer.color;
            }

            _currentHealth = _maxHealth;
        }

        public void TakeDamage(float amount, Vector2 knockbackDirection, float knockbackForce = 0f)
        {
            if (_isDead) return;

            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            // Apply knockback
            if (knockbackForce > 0f && _rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);
            }

            TriggerFlash();

            if (_currentHealth <= 0f)
            {
                Die();
            }
        }

        private void TriggerFlash()
        {
            if (_spriteRenderer == null) return;
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            _spriteRenderer.color = _flashColor;
            yield return new WaitForSeconds(0.1f);
            _spriteRenderer.color = _originalColor;
            _flashRoutine = null;
        }

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;

            if (_deathSound != null)
            {
                AudioSource.PlayClipAtPoint(_deathSound, transform.position, 0.8f);
            }

            // Drop wax shard reward
            if (_waxShardPrefab != null && UnityEngine.Random.value <= _dropChance)
            {
                Instantiate(_waxShardPrefab, transform.position, Quaternion.identity);
            }

            OnDied?.Invoke();

            var hud = UnityEngine.Object.FindFirstObjectByType<WickFall.UI.GameHUDManager>();
            if (hud != null)
            {
                hud.RegisterEnemyDefeated();
            }

            Destroy(gameObject);
        }
    }
}
