using UnityEngine;

namespace WickFall.Player
{
    /// <summary>
    /// Dynamically alters the Candle Golem's physical appearance in the game world
    /// as its wax (health pool) depletes.
    /// Provides organic melting squash/stretch, molten wax shading tint,
    /// and repositioning of the wick light and collider bounds.
    /// </summary>
    public class PlayerVisualMelter : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private PlayerWax _playerWax;
        [SerializeField] private CircleCollider2D _collider;
        [SerializeField] private Transform _wickLightTransform;

        [Header("Melting Deform Bounds")]
        [SerializeField] private Vector2 _fullScale = new Vector2(1.0f, 1.0f);
        [SerializeField] private Vector2 _meltedScale = new Vector2(1.28f, 0.58f); // Squashed & melted wide

        [Header("Height Offset Bounds")]
        [SerializeField] private float _fullWickHeight = 0.35f;
        [SerializeField] private float _meltedWickHeight = 0.12f;

        [Header("Shading Tiers (Pristine to Charred Ashen)")]
        [SerializeField] private Color _tier1Color = new Color(1.0f, 1.0f, 1.0f, 1.0f);       // Pristine ivory
        [SerializeField] private Color _tier2Color = new Color(0.96f, 0.90f, 0.80f, 1.0f);   // Molten cream
        [SerializeField] private Color _tier3Color = new Color(0.82f, 0.72f, 0.60f, 1.0f);   // Charred soot
        [SerializeField] private Color _tier4Color = new Color(0.60f, 0.50f, 0.44f, 1.0f);   // Dying ashen stub
        [SerializeField] private Color _hurtColor = new Color(1.0f, 0.22f, 0.22f, 1.0f);     // Damage flash red

        private Vector2 _targetScale;
        private Vector2 _currentScale;
        private Color _targetColor;
        private Color _currentColor;
        private float _targetWickY;
        private float _currentWickY;
        private float _hurtFlashTimer;

        public void TriggerHurtFlash(float duration = 0.45f)
        {
            _hurtFlashTimer = duration;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _hurtColor;
            }
        }

        private void Awake()
        {
            if (_playerWax == null) _playerWax = GetComponent<PlayerWax>();
            if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (_collider == null) _collider = GetComponent<CircleCollider2D>();

            if (_hurtColor == Color.clear || _hurtColor.a < 0.1f)
            {
                _hurtColor = new Color(1.0f, 0.15f, 0.15f, 1.0f);
            }

            _targetScale = _fullScale;
            _currentScale = _fullScale;
            _targetColor = _tier1Color;
            _currentColor = _tier1Color;
            _targetWickY = _fullWickHeight;
            _currentWickY = _fullWickHeight;
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

        private void HandlePlayerExtinguished()
        {
            enabled = false;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.transform.localScale = Vector3.one;
                _spriteRenderer.color = Color.white;
            }
        }

        private void Update()
        {
            // Smoothly deform sprite transform
            _currentScale = Vector2.Lerp(_currentScale, _targetScale, Time.deltaTime * 4.0f);
            if (_spriteRenderer != null)
            {
                _spriteRenderer.transform.localScale = new Vector3(_currentScale.x, _currentScale.y, 1f);

                _currentColor = Color.Lerp(_currentColor, _targetColor, Time.deltaTime * 3.5f);

                if (_hurtFlashTimer > 0f)
                {
                    _hurtFlashTimer -= Time.deltaTime;
                    _spriteRenderer.color = _hurtColor;
                }
                else
                {
                    _spriteRenderer.color = _currentColor;
                }
            }

            // Adjust light and collider height
            _currentWickY = Mathf.Lerp(_currentWickY, _targetWickY, Time.deltaTime * 4.0f);
            if (_wickLightTransform != null)
            {
                Vector3 lp = _wickLightTransform.localPosition;
                lp.y = _currentWickY;
                _wickLightTransform.localPosition = lp;
            }

            if (_collider != null)
            {
                _collider.offset = new Vector2(0f, _currentWickY * 0.45f);
            }
        }

        private void HandleWaxChanged(float current, float max)
        {
            if (max <= 0f) return;

            float ratio = Mathf.Clamp01(current / max);

            // Interpolate melting squash
            _targetScale = Vector2.Lerp(_meltedScale, _fullScale, ratio);
            _targetWickY = Mathf.Lerp(_meltedWickHeight, _fullWickHeight, ratio);

            // Determine color shading tier
            if (ratio > 0.75f)
            {
                _targetColor = _tier1Color;
            }
            else if (ratio > 0.50f)
            {
                _targetColor = _tier2Color;
            }
            else if (ratio > 0.25f)
            {
                _targetColor = _tier3Color;
            }
            else
            {
                _targetColor = _tier4Color;
            }
        }
    }
}
