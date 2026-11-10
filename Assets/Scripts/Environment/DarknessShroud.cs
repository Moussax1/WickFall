using System.Collections;
using UnityEngine;
using WickFall.Player;

namespace WickFall.Environment
{
    /// <summary>
    /// Implements the dynamic 2D Darkness Shroud / Fog of War.
    /// Shrouds the catacombs in pitch-black darkness outside the Candle Golem's flame.
    /// Dynamically centers on the player's candle wick, scaling its aperture with remaining wax.
    /// Cleanses into radiant full visibility upon kindling the sacred Brazier.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class DarknessShroud : MonoBehaviour
    {
        [Header("Target Tracking")]
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private Vector3 _wickOffset = new Vector3(0f, 0.35f, 0f);

        [Header("Aperture Scaling")]
        [SerializeField] private float _maxScale = 1.0f; // Full wax visibility (approx 7.5m bright radius)
        [SerializeField] private float _minScale = 0.38f; // Critical wax near-blackness (approx 2.5m bright radius)
        [SerializeField] private float _lerpSpeed = 3.5f;

        [Header("Flicker Dynamics")]
        [SerializeField] private float _flickerSpeed = 10f;
        [SerializeField] private float _flickerMagnitude = 0.05f;

        private SpriteRenderer _sr;
        private PlayerWax _playerWax;
        private float _targetScale;
        private float _currentScale;
        private bool _isCleansed;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _sr.sortingOrder = 50; // Render above world, below HUD
            _targetScale = _maxScale;
            _currentScale = _maxScale;
            transform.localScale = new Vector3(_maxScale, _maxScale, 1f);
        }

        private void Start()
        {
            if (_playerTransform == null)
            {
                var player = Object.FindFirstObjectByType<PlayerController>();
                if (player != null)
                {
                    _playerTransform = player.transform;
                    _playerWax = player.GetComponent<PlayerWax>();
                }
            }

            if (_playerWax != null)
            {
                _playerWax.OnWaxChanged += HandleWaxChanged;
                _playerWax.OnPlayerExtinguished += HandlePlayerExtinguished;
            }

            Brazier.OnBrazierKindled += HandleBrazierKindled;
        }

        private void OnDestroy()
        {
            if (_playerWax != null)
            {
                _playerWax.OnWaxChanged -= HandleWaxChanged;
                _playerWax.OnPlayerExtinguished -= HandlePlayerExtinguished;
            }

            Brazier.OnBrazierKindled -= HandleBrazierKindled;
        }

        private void LateUpdate()
        {
            if (_playerTransform != null && !_isCleansed)
            {
                // Lock position directly onto player's flickering candle wick
                Vector3 targetPos = _playerTransform.position + _wickOffset;
                targetPos.z = -1f; // In front of tiles and sprites
                transform.position = targetPos;
            }

            if (_isCleansed) return;

            // Smooth scale interpolation
            _currentScale = Mathf.Lerp(_currentScale, _targetScale, Time.deltaTime * _lerpSpeed);

            // Procedural flame breathing / flicker
            float perlin = (Mathf.PerlinNoise(Time.time * _flickerSpeed, 0.5f) - 0.5f) * 2f;
            float flickerFactor = 1f + perlin * _flickerMagnitude;

            float finalScale = _currentScale * flickerFactor;
            transform.localScale = new Vector3(finalScale, finalScale, 1f);
        }

        private void HandleWaxChanged(float current, float max)
        {
            if (_isCleansed || max <= 0f) return;

            float ratio = Mathf.Clamp01(current / max);
            _targetScale = Mathf.Lerp(_minScale, _maxScale, ratio);
        }

        private void HandlePlayerExtinguished()
        {
            if (_isCleansed) return;
            // Shroud rapidly closes in on death
            _targetScale = 0.15f;
        }

        private void HandleBrazierKindled()
        {
            _isCleansed = true;
            StartCoroutine(CleanseRoutine());
        }

        private IEnumerator CleanseRoutine()
        {
            float elapsed = 0f;
            float duration = 2.0f;
            Vector3 startScale = transform.localScale;
            Vector3 endScale = new Vector3(80f, 80f, 1f);
            Color startColor = _sr.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                transform.localScale = Vector3.Lerp(startScale, endScale, t);
                _sr.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0f, t));
                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
