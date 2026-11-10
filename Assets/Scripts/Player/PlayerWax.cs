using System;
using System.Collections;
using UnityEngine;

namespace WickFall.Player
{
    /// <summary>
    /// Core survival and health system. Wax represents both the Golem's life and its flame.
    /// Emits decoupled C# events for UI, lighting, and audio listeners.
    /// </summary>
    public class PlayerWax : MonoBehaviour
    {
        [Header("Wax Settings")]
        [SerializeField] private float _maxWax = 100f;
        [SerializeField] private float _currentWax = 100f;
        [SerializeField] private float _passiveBurnRate = 1.0f; // Units per second
        [SerializeField] private float _invulnerabilityDuration = 0.6f;

        [Header("Audio")]
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioClip _hurtSound;
        [SerializeField] private AudioClip _extinguishSound;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _hurtColor = new Color(1f, 0.35f, 0.35f, 1f);

        private bool _isDead;
        private bool _isImmortal;
        private float _lastDamageTime = -10f;
        private Color _originalColor = Color.white;
        private Coroutine _hurtFlashCoroutine;
        private PlayerController _playerController;

        public float MaxWax => _maxWax;
        public float CurrentWax => _currentWax;
        public float WaxRatio => Mathf.Clamp01(_currentWax / _maxWax);
        public bool IsDead => _isDead;

        public event Action<float, float> OnWaxChanged;
        public event Action OnPlayerExtinguished;
        public event Action<float> OnPlayerDamaged;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (_spriteRenderer != null)
            {
                _originalColor = _spriteRenderer.color;
            }

            _currentWax = _maxWax;
            _isDead = false;
        }

        private void Start()
        {
            OnWaxChanged?.Invoke(_currentWax, _maxWax);
        }

        private void Update()
        {
            if (_isDead || _isImmortal) return;

            if (_passiveBurnRate > 0f)
            {
                _currentWax = Mathf.Max(0f, _currentWax - _passiveBurnRate * Time.deltaTime);
                OnWaxChanged?.Invoke(_currentWax, _maxWax);

                if (_currentWax <= 0f)
                {
                    Extinguish();
                }
            }
        }

        public void TakeDamage(float amount, Vector2 knockbackDirection = default, float knockbackForce = 0f)
        {
            if (_isDead || _isImmortal) return;
            if (Time.time < _lastDamageTime + _invulnerabilityDuration) return;

            _lastDamageTime = Time.time;
            _currentWax = Mathf.Max(0f, _currentWax - amount);

            if (_sfxSource != null && _hurtSound != null)
            {
                _sfxSource.PlayOneShot(_hurtSound);
            }

            if (knockbackForce > 0f && _playerController != null && knockbackDirection != Vector2.zero)
            {
                _playerController.ApplyImpulse(knockbackDirection.normalized * knockbackForce);
            }

            TriggerHurtFlash();

            OnWaxChanged?.Invoke(_currentWax, _maxWax);
            OnPlayerDamaged?.Invoke(amount);

            if (_currentWax <= 0f)
            {
                Extinguish();
            }
        }

        public void DrainWax(float amount)
        {
            if (_isDead || _isImmortal || amount <= 0f) return;

            _currentWax = Mathf.Max(0f, _currentWax - amount);
            OnWaxChanged?.Invoke(_currentWax, _maxWax);

            if (_currentWax <= 0f)
            {
                Extinguish();
            }
        }

        public void RestoreWax(float amount)
        {
            if (_isDead) return;

            _currentWax = Mathf.Clamp(_currentWax + amount, 0f, _maxWax);
            OnWaxChanged?.Invoke(_currentWax, _maxWax);
        }

        public void SetImmortal(bool immortal)
        {
            _isImmortal = immortal;
        }

        private void TriggerHurtFlash()
        {
            var melter = GetComponent<PlayerVisualMelter>();
            if (melter != null)
            {
                melter.TriggerHurtFlash(0.45f);
                return;
            }

            if (_spriteRenderer == null) return;
            if (_hurtFlashCoroutine != null) StopCoroutine(_hurtFlashCoroutine);
            _hurtFlashCoroutine = StartCoroutine(HurtFlashRoutine());
        }

        private IEnumerator HurtFlashRoutine()
        {
            _spriteRenderer.color = _hurtColor;
            yield return new WaitForSeconds(0.12f);
            _spriteRenderer.color = _originalColor;
            yield return new WaitForSeconds(0.08f);
            _spriteRenderer.color = _hurtColor;
            yield return new WaitForSeconds(0.12f);
            _spriteRenderer.color = _originalColor;
            _hurtFlashCoroutine = null;
        }

        private void Extinguish()
        {
            if (_isDead) return;
            _isDead = true;

            if (_sfxSource != null && _extinguishSound != null)
            {
                _sfxSource.PlayOneShot(_extinguishSound);
            }

            if (_playerController != null)
            {
                _playerController.SetCanMove(false);
            }

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = new Color(0.35f, 0.35f, 0.35f, 0.8f); // Ashen charred color
            }

            OnPlayerExtinguished?.Invoke();
        }
    }
}
