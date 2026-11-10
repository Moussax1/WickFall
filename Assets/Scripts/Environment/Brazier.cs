using System;
using UnityEngine;
using WickFall.Player;

namespace WickFall.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class Brazier : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite _unlitSprite;
        [SerializeField] private Sprite _litSprite;
        [SerializeField] private Light _brazierLight;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _kindleSound;
        [SerializeField] private bool _autoKindleOnTouch = true;

        private bool _isKindled;
        public bool IsKindled => _isKindled;
        public static event Action OnBrazierKindled;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_unlitSprite != null && _spriteRenderer != null) _spriteRenderer.sprite = _unlitSprite;
            if (_brazierLight != null) _brazierLight.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isKindled || !_autoKindleOnTouch) return;
            if (other.TryGetComponent<PlayerWax>(out var wax) && !wax.IsDead)
                Kindle(wax);
        }

        public void Kindle(PlayerWax playerWax)
        {
            if (_isKindled) return;
            _isKindled = true;

            if (_spriteRenderer != null && _litSprite != null) _spriteRenderer.sprite = _litSprite;
            if (_brazierLight != null) _brazierLight.enabled = true;
            if (_audioSource != null && _kindleSound != null) _audioSource.PlayOneShot(_kindleSound);

            if (playerWax != null)
            {
                playerWax.RestoreWax(playerWax.MaxWax);
                playerWax.SetImmortal(true);
            }

            OnBrazierKindled?.Invoke();

            var hud = UnityEngine.Object.FindFirstObjectByType<WickFall.UI.GameHUDManager>();
            if (hud != null) hud.ShowVictory();
        }
    }
}
