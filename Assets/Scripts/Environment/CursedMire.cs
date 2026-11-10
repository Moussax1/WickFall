using UnityEngine;
using WickFall.Player;

namespace WickFall.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class CursedMire : MonoBehaviour
    {
        [Header("Mire Settings")]
        [SerializeField] private float _speedMultiplier = 0.6f;
        [SerializeField] private float _waxDrainPerSecond = 3.5f;

        [Header("Visuals & Audio")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _sludgeColor = new Color(0.22f, 0.08f, 0.35f, 0.92f);
        [SerializeField] private Color _sludgeGlowColor = new Color(0.35f, 0.12f, 0.52f, 0.95f);
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioClip _sizzleSound;

        private PlayerController _activePlayer;
        private PlayerWax _activeWax;
        private SpriteRenderer _playerSR;
        private Color _playerOriginalColor = Color.white;
        private Vector3 _baseScale;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sortingOrder = -5;
                _spriteRenderer.color = _sludgeColor;
            }
            _baseScale = transform.localScale;
        }

        private void Update()
        {
            if (_spriteRenderer != null && _baseScale != Vector3.zero)
            {
                // Organic bubbling / breathing pulsation so players clearly identify active cursed sludge
                float pulse = 1f + (Mathf.Sin(Time.time * 2.5f) * 0.04f);
                transform.localScale = new Vector3(_baseScale.x * pulse, _baseScale.y * pulse, 1f);

                float colorT = (Mathf.Sin(Time.time * 3.0f) + 1f) * 0.5f;
                _spriteRenderer.color = Color.Lerp(_sludgeColor, _sludgeGlowColor, colorT);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerController>(out var player))
            {
                _activePlayer = player;
                _activeWax = other.GetComponent<PlayerWax>();
                _activePlayer.SetSpeedMultiplier(_speedMultiplier);

                _playerSR = other.GetComponentInChildren<SpriteRenderer>();
                if (_playerSR != null)
                {
                    _playerSR.color = new Color(0.65f, 0.5f, 0.85f, 1f); // Cursed sludge tint feedback
                }

                if (_sfxSource != null && _sizzleSound != null && !_sfxSource.isPlaying)
                {
                    _sfxSource.Play();
                }
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (_activeWax == null && other.TryGetComponent<PlayerWax>(out var wax))
            {
                _activeWax = wax;
            }

            if (_activeWax != null && !_activeWax.IsDead)
            {
                _activeWax.DrainWax(_waxDrainPerSecond * Time.deltaTime);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerController>(out var player) && player == _activePlayer)
            {
                ReleasePlayer();
            }
        }

        private void OnDisable()
        {
            ReleasePlayer();
        }

        private void ReleasePlayer()
        {
            if (_activePlayer != null)
            {
                _activePlayer.SetSpeedMultiplier(1.0f);
                _activePlayer = null;
            }

            if (_playerSR != null)
            {
                _playerSR.color = Color.white;
                _playerSR = null;
            }

            _activeWax = null;

            if (_sfxSource != null && _sfxSource.isPlaying)
            {
                _sfxSource.Stop();
            }
        }
    }
}
