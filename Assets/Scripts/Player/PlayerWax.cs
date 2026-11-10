using System;
using UnityEngine;

namespace WickFall.Player
{
    public class PlayerWax : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _maxWax = 100f;
        [SerializeField] private float _currentWax = 100f;
        [SerializeField] private float _passiveBurnRate = 1.0f;
        [SerializeField] private float _invulnerabilityDuration = 0.5f;

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
        private PlayerController _controller;
        private PlayerVisualMelter _melter;

        public float MaxWax => _maxWax;
        public float CurrentWax => _currentWax;
        public float WaxRatio => Mathf.Clamp01(_currentWax / Mathf.Max(1f, _maxWax));
        public bool IsDead => _isDead;

        public event Action<float, float> OnWaxChanged;
        public event Action OnPlayerExtinguished;
        public event Action<float> OnPlayerDamaged;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _melter = GetComponent<PlayerVisualMelter>();
            if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _currentWax = _maxWax;
        }

        private void Start() => OnWaxChanged?.Invoke(_currentWax, _maxWax);

        private void Update()
        {
            if (_isDead || _isImmortal || _passiveBurnRate <= 0f) return;
            DrainWax(_passiveBurnRate * Time.deltaTime);
        }

        public void TakeDamage(float amount, Vector2 knockbackDirection = default, float knockbackForce = 0f)
        {
            if (_isDead || _isImmortal || Time.time < _lastDamageTime + _invulnerabilityDuration) return;
            _lastDamageTime = Time.time;

            if (_sfxSource != null && _hurtSound != null) _sfxSource.PlayOneShot(_hurtSound);

            if (knockbackForce > 0f && _controller != null && knockbackDirection != Vector2.zero)
                _controller.ApplyImpulse(knockbackDirection.normalized * knockbackForce);

            if (_melter != null) _melter.TriggerHurtFlash();

            _currentWax = Mathf.Max(0f, _currentWax - amount);
            OnWaxChanged?.Invoke(_currentWax, _maxWax);
            OnPlayerDamaged?.Invoke(amount);

            if (_currentWax <= 0f) Extinguish();
        }

        public void DrainWax(float amount)
        {
            if (_isDead || _isImmortal || amount <= 0f) return;
            _currentWax = Mathf.Max(0f, _currentWax - amount);
            OnWaxChanged?.Invoke(_currentWax, _maxWax);
            if (_currentWax <= 0f) Extinguish();
        }

        public void RestoreWax(float amount)
        {
            if (_isDead) return;
            _currentWax = Mathf.Clamp(_currentWax + amount, 0f, _maxWax);
            OnWaxChanged?.Invoke(_currentWax, _maxWax);
        }

        public void SetImmortal(bool immortal) => _isImmortal = immortal;

        private void Extinguish()
        {
            if (_isDead) return;
            _isDead = true;

            if (_sfxSource != null && _extinguishSound != null) _sfxSource.PlayOneShot(_extinguishSound);
            if (_controller != null) _controller.SetCanMove(false);
            if (_spriteRenderer != null) _spriteRenderer.color = new Color(0.35f, 0.35f, 0.35f, 0.8f);

            OnPlayerExtinguished?.Invoke();
        }
    }
}
