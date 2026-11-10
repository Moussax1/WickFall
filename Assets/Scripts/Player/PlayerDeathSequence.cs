using System;
using System.Collections;
using UnityEngine;
using WickFall.UI;

namespace WickFall.Player
{
    /// <summary>
    /// Executes the 4-phase sequential melting death animation when the Golem's wax expires.
    /// Locks movement, extinguishes flame, collapses into an organic wax puddle,
    /// and smoothly triggers the Game Over screen.
    /// </summary>
    [RequireComponent(typeof(PlayerWax))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerDeathSequence : MonoBehaviour
    {
        [Header("Melt Animation Frames")]
        [SerializeField] private Sprite[] _meltFrames; // 4 sequential death frames

        [Header("Frame Durations")]
        [SerializeField] private float _frame0Duration = 0.28f; // Smoke puff & snap
        [SerializeField] private float _frame1Duration = 0.35f; // Slumping torso
        [SerializeField] private float _frame2Duration = 0.40f; // Kneeling collapse
        [SerializeField] private float _frame3HoldDuration = 0.70f; // Final cold puddle hold

        [Header("Audio")]
        [SerializeField] private AudioClip _extinguishPuffSound;

        private PlayerWax _playerWax;
        private SpriteRenderer _sr;
        private PlayerController _controller;
        private PlayerAttack _attack;
        private CandleLight _candleLight;
        private Rigidbody2D _rb;
        private bool _isDying;

        public static event Action OnDeathSequenceFinished;

        private void Awake()
        {
            _playerWax = GetComponent<PlayerWax>();
            _sr = GetComponent<SpriteRenderer>();
            _controller = GetComponent<PlayerController>();
            _attack = GetComponent<PlayerAttack>();
            _candleLight = GetComponentInChildren<CandleLight>();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            if (_playerWax != null)
            {
                _playerWax.OnPlayerExtinguished += StartDeathSequence;
            }
        }

        private void OnDisable()
        {
            if (_playerWax != null)
            {
                _playerWax.OnPlayerExtinguished -= StartDeathSequence;
            }
        }

        public void StartDeathSequence()
        {
            if (_isDying) return;
            _isDying = true;

            // Lock input and stop momentum immediately
            if (_controller != null) _controller.enabled = false;
            if (_attack != null) _attack.enabled = false;
            if (_rb != null) _rb.linearVelocity = Vector2.zero;

            StartCoroutine(MeltRoutine());
        }

        private IEnumerator MeltRoutine()
        {
            // Frame 0: Flame sputters, black smoke puff
            if (_meltFrames != null && _meltFrames.Length > 0 && _meltFrames[0] != null)
            {
                _sr.sprite = _meltFrames[0];
            }

            if (_extinguishPuffSound != null)
            {
                AudioSource.PlayClipAtPoint(_extinguishPuffSound, transform.position, 0.9f);
            }

            yield return new WaitForSecondsRealtime(_frame0Duration);

            // Frame 1: Head slumps into torso
            if (_meltFrames != null && _meltFrames.Length > 1 && _meltFrames[1] != null)
            {
                _sr.sprite = _meltFrames[1];
            }
            yield return new WaitForSecondsRealtime(_frame1Duration);

            // Frame 2: Kneeling collapse into dripping mound
            if (_meltFrames != null && _meltFrames.Length > 2 && _meltFrames[2] != null)
            {
                _sr.sprite = _meltFrames[2];
            }
            yield return new WaitForSecondsRealtime(_frame2Duration);

            // Frame 3: Cold flat puddle with empty boots and stub
            if (_meltFrames != null && _meltFrames.Length > 3 && _meltFrames[3] != null)
            {
                _sr.sprite = _meltFrames[3];
            }

            // Darkness Shroud pinches down over the puddle
            yield return new WaitForSecondsRealtime(_frame3HoldDuration);

            OnDeathSequenceFinished?.Invoke();

            var hud = UnityEngine.Object.FindFirstObjectByType<GameHUDManager>();
            if (hud != null)
            {
                hud.ShowGameOver();
            }
        }
    }
}
