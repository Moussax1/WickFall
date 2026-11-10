using UnityEngine;

namespace WickFall.Player
{
    public class CandleLight : MonoBehaviour
    {
        [SerializeField] private PlayerWax _playerWax;
        [SerializeField] private Light _pointLight;
        [SerializeField] private float _maxRange = 8.0f;
        [SerializeField] private float _minRange = 2.0f;
        [SerializeField] private float _maxIntensity = 1.5f;
        [SerializeField] private float _minIntensity = 0.4f;
        [SerializeField] private float _flickerSpeed = 10f;
        [SerializeField] private float _flickerAmount = 0.1f;
        [SerializeField] private Color _flameColor = new Color(1.0f, 0.65f, 0.28f, 1f);

        private float _externalDistortion = 1.0f;

        private void Awake()
        {
            if (_playerWax == null) _playerWax = GetComponentInParent<PlayerWax>();
            if (_pointLight == null)
            {
                _pointLight = GetComponent<Light>();
                if (_pointLight == null) _pointLight = gameObject.AddComponent<Light>();
            }

            if (_pointLight != null)
            {
                _pointLight.type = LightType.Point;
                _pointLight.color = _flameColor;
            }
        }

        public void SetFlameDistortion(float multiplier)
        {
            _externalDistortion = Mathf.Max(_externalDistortion, multiplier);
        }

        private void Update()
        {
            if (_pointLight == null) return;

            _externalDistortion = Mathf.MoveTowards(_externalDistortion, 1.0f, Time.deltaTime * 3.0f);
            float ratio = _playerWax != null ? _playerWax.WaxRatio : 1f;
            float flicker = (Mathf.PerlinNoise(Time.time * _flickerSpeed * _externalDistortion, 0f) - 0.5f) * _flickerAmount * _externalDistortion;

            _pointLight.range = Mathf.Lerp(_minRange, _maxRange, ratio);
            _pointLight.intensity = Mathf.Max(0f, Mathf.Lerp(_minIntensity, _maxIntensity, ratio) + flicker);
        }
    }
}
