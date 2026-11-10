using UnityEngine;

namespace WickFall.Player
{
    /// <summary>
    /// Controls the dynamic point light attached to the Candle Golem's wick.
    /// Range and intensity scale smoothly with remaining wax pool.
    /// Provides subtle procedural flame flicker and erratic drops when wax is critical (<25%).
    /// </summary>
    public class CandleLight : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerWax _playerWax;
        [SerializeField] private Light _pointLight;

        [Header("Range Bounds")]
        [SerializeField] private float _maxRange = 8.0f;
        [SerializeField] private float _minRange = 2.0f;

        [Header("Intensity Bounds")]
        [SerializeField] private float _maxIntensity = 1.5f;
        [SerializeField] private float _minIntensity = 0.4f;

        [Header("Flame Flicker Settings")]
        [SerializeField] private float _flickerSpeed = 12f;
        [SerializeField] private float _flickerAmount = 0.15f;
        [SerializeField] private Color _flameColor = new Color(1.0f, 0.65f, 0.28f, 1f);

        private float _targetRange;
        private float _targetIntensity;
        private float _currentRange;
        private float _currentIntensity;
        private bool _isExtinguished;
        private float _externalDistortion = 1.0f;

        public void SetFlameDistortion(float multiplier)
        {
            _externalDistortion = Mathf.Max(_externalDistortion, multiplier);
        }

        private void Awake()
        {
            if (_playerWax == null)
            {
                _playerWax = GetComponentInParent<PlayerWax>();
            }

            if (_pointLight == null)
            {
                _pointLight = GetComponent<Light>();
                if (_pointLight == null)
                {
                    _pointLight = gameObject.AddComponent<Light>();
                }
            }

            _pointLight.type = LightType.Point;
            _pointLight.color = _flameColor;
            _pointLight.shadows = LightShadows.None;

            _targetRange = _maxRange;
            _targetIntensity = _maxIntensity;
            _currentRange = _maxRange;
            _currentIntensity = _maxIntensity;
        }

        private void OnEnable()
        {
            if (_playerWax != null)
            {
                _playerWax.OnWaxChanged += HandleWaxChanged;
                _playerWax.OnPlayerExtinguished += HandlePlayerExtinguished;
            }
        }

        private void OnDisable()
        {
            if (_playerWax != null)
            {
                _playerWax.OnWaxChanged -= HandleWaxChanged;
                _playerWax.OnPlayerExtinguished -= HandlePlayerExtinguished;
            }
        }

        private void Update()
        {
            if (_isExtinguished || _pointLight == null) return;

            // Interpolate toward target values
            _currentRange = Mathf.Lerp(_currentRange, _targetRange, Time.deltaTime * 3.5f);
            _currentIntensity = Mathf.Lerp(_currentIntensity, _targetIntensity, Time.deltaTime * 3.5f);

            // Decay external distortion smoothly back to normal
            _externalDistortion = Mathf.MoveTowards(_externalDistortion, 1.0f, Time.deltaTime * 3.0f);

            // Perlin noise flicker
            float perlin = (Mathf.PerlinNoise(Time.time * _flickerSpeed * _externalDistortion, 0f) - 0.5f) * 2f;
            float flicker = perlin * _flickerAmount * _externalDistortion;

            // Critical wax (<25%) introduces erratic sputtering
            float waxRatio = _playerWax != null ? _playerWax.WaxRatio : 1f;
            if (waxRatio < 0.25f)
            {
                float criticalJitter = (Random.value - 0.5f) * 0.4f;
                // Chance of sudden micro-fade
                if (Random.value < 0.08f)
                {
                    criticalJitter -= 0.35f;
                }
                flicker += criticalJitter;
            }

            _pointLight.range = Mathf.Max(0.5f, _currentRange + flicker * 0.8f);
            _pointLight.intensity = Mathf.Max(0.1f, _currentIntensity + flicker);
        }

        private void HandleWaxChanged(float currentWax, float maxWax)
        {
            if (_isExtinguished) return;

            float ratio = maxWax > 0f ? Mathf.Clamp01(currentWax / maxWax) : 0f;
            _targetRange = Mathf.Lerp(_minRange, _maxRange, ratio);
            _targetIntensity = Mathf.Lerp(_minIntensity, _maxIntensity, ratio);
        }

        private void HandlePlayerExtinguished()
        {
            _isExtinguished = true;
            if (_pointLight != null)
            {
                _pointLight.intensity = 0f;
                _pointLight.range = 0f;
            }
        }
    }
}
