using UnityEngine;
using WickFall.Player;

namespace WickFall.Environment
{
    /// <summary>
    /// Cursed shadow-wax sludge pool on the dungeon floor.
    /// Entangles the player, slowing movement by 40% and slowly draining wax while inside.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CursedMire : MonoBehaviour
    {
        [Header("Mire Settings")]
        [SerializeField] private float _speedMultiplier = 0.6f;
        [SerializeField] private float _waxDrainPerSecond = 3.5f;

        [Header("Visuals & Audio")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioClip _sizzleSound;

        private PlayerController _activePlayer;
        private PlayerWax _activeWax;

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
                _spriteRenderer.color = new Color(0.2f, 0.14f, 0.28f, 0.85f); // Viscous dark purple shadow
                _spriteRenderer.sortingOrder = -5; // Above floor, below player
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerController>(out var player))
            {
                _activePlayer = player;
                _activeWax = other.GetComponent<PlayerWax>();
                _activePlayer.SetSpeedMultiplier(_speedMultiplier);

                if (_sfxSource != null && _sizzleSound != null && !_sfxSource.isPlaying)
                {
                    _sfxSource.Play();
                }
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (_activeWax != null && !_activeWax.IsDead)
            {
                _activeWax.TakeDamage(_waxDrainPerSecond * Time.deltaTime);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerController>(out var player) && player == _activePlayer)
            {
                _activePlayer.SetSpeedMultiplier(1.0f);
                _activePlayer = null;
                _activeWax = null;

                if (_sfxSource != null && _sfxSource.isPlaying)
                {
                    _sfxSource.Stop();
                }
            }
        }

        private void OnDisable()
        {
            if (_activePlayer != null)
            {
                _activePlayer.SetSpeedMultiplier(1.0f);
                _activePlayer = null;
                _activeWax = null;
            }
        }
    }
}
