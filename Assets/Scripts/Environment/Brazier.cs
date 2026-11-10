using System;
using System.Collections;
using UnityEngine;
using WickFall.Player;

namespace WickFall.Environment
{
    /// <summary>
    /// The sacred Brazier at the dungeon's sanctum.
    /// Igniting it restores the Golem's wax completely, cleanses the dark catacombs with radiant light,
    /// and triggers the Victory flow.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Brazier : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite _unlitSprite;
        [SerializeField] private Sprite _litSprite;

        [Header("Lighting")]
        [SerializeField] private Light _brazierLight;
        [SerializeField] private float _targetLightRange = 16f;
        [SerializeField] private float _targetLightIntensity = 2.2f;
        [SerializeField] private Color _litLightColor = new Color(1.0f, 0.72f, 0.35f, 1f);

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _kindleSound;

        [Header("Interaction")]
        [SerializeField] private bool _autoKindleOnTouch = true;

        private bool _isKindled;

        public bool IsKindled => _isKindled;
        public static event Action OnBrazierKindled;

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;

            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_unlitSprite != null && _spriteRenderer != null)
            {
                _spriteRenderer.sprite = _unlitSprite;
            }

            if (_brazierLight != null)
            {
                _brazierLight.type = LightType.Point;
                _brazierLight.intensity = 0f;
                _brazierLight.range = 0f;
                _brazierLight.color = _litLightColor;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isKindled) return;

            if (other.TryGetComponent<PlayerWax>(out var playerWax))
            {
                if (playerWax.IsDead) return;

                if (_autoKindleOnTouch)
                {
                    Kindle(playerWax);
                }
            }
        }

        public void Kindle(PlayerWax playerWax)
        {
            if (_isKindled) return;
            _isKindled = true;

            // Update visuals
            if (_spriteRenderer != null && _litSprite != null)
            {
                _spriteRenderer.sprite = _litSprite;
            }

            // Audio
            if (_audioSource != null && _kindleSound != null)
            {
                _audioSource.PlayOneShot(_kindleSound);
            }

            // Restore player to max wax & grant immortality for the victory celebration
            if (playerWax != null)
            {
                playerWax.RestoreWax(playerWax.MaxWax);
                playerWax.SetImmortal(true);
            }

            StartCoroutine(RadiantBurstRoutine());

            OnBrazierKindled?.Invoke();
        }

        private IEnumerator RadiantBurstRoutine()
        {
            if (_brazierLight == null) yield break;

            float elapsed = 0f;
            float duration = 1.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                _brazierLight.range = Mathf.Lerp(0f, _targetLightRange, t);
                _brazierLight.intensity = Mathf.Lerp(0f, _targetLightIntensity, t);
                yield return null;
            }
        }
    }
}
