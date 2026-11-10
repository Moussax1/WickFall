using System;
using System.Collections;
using UnityEngine;

namespace WickFall.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private float _maxHealth = 50f;
        [SerializeField] private float _currentHealth = 50f;
        [SerializeField] private GameObject _waxShardPrefab;
        [Range(0f, 1f)] [SerializeField] private float _dropChance = 0.65f;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _flashColor = new Color(1f, 0.4f, 0.4f, 1f);
        [SerializeField] private AudioClip _deathSound;

        private Rigidbody2D _rb;
        private bool _isDead;
        private Coroutine _flashRoutine;
        private Color _baseColor = Color.white;

        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public bool IsDead => _isDead;

        public event Action<float, float> OnHealthChanged;
        public event Action OnDied;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (_spriteRenderer != null) _baseColor = _spriteRenderer.color;
            _currentHealth = _maxHealth;
        }

        public void TakeDamage(float amount, Vector2 knockbackDir, float knockbackForce = 0f)
        {
            if (_isDead) return;

            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            if (knockbackForce > 0f && _rb != null)
                _rb.AddForce(knockbackDir.normalized * knockbackForce, ForceMode2D.Impulse);

            if (_spriteRenderer != null)
            {
                if (_flashRoutine != null) StopCoroutine(_flashRoutine);
                _flashRoutine = StartCoroutine(Flash());
            }

            if (_currentHealth <= 0f) Die();
        }

        private IEnumerator Flash()
        {
            _spriteRenderer.color = _flashColor;
            yield return new WaitForSeconds(0.1f);
            _spriteRenderer.color = _baseColor;
            _flashRoutine = null;
        }

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;

            if (_deathSound != null) AudioSource.PlayClipAtPoint(_deathSound, transform.position);

            if (_waxShardPrefab != null && UnityEngine.Random.value <= _dropChance)
                Instantiate(_waxShardPrefab, transform.position, Quaternion.identity);

            OnDied?.Invoke();

            var hud = UnityEngine.Object.FindFirstObjectByType<WickFall.UI.GameHUDManager>();
            if (hud != null) hud.RegisterEnemyDefeated();

            Destroy(gameObject, 0.05f);
        }

        private void OnDisable()
        {
            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
                _flashRoutine = null;
            }
            if (_spriteRenderer != null) _spriteRenderer.color = _baseColor;
        }
    }
}
