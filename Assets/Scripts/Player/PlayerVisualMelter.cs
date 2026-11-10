using System.Collections;
using UnityEngine;

namespace WickFall.Player
{
    public class PlayerVisualMelter : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private PlayerWax _playerWax;
        [SerializeField] private CircleCollider2D _collider;
        [SerializeField] private Transform _wickLightTransform;
        [SerializeField] private Color _hurtColor = new Color(1f, 0.2f, 0.2f, 1f);

        private Coroutine _flashRoutine;
        private Color _baseColor = Color.white;

        private void Awake()
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (_playerWax == null) _playerWax = GetComponent<PlayerWax>();
            if (_spriteRenderer != null) _baseColor = _spriteRenderer.color;
        }

        public void TriggerHurtFlash(float duration = 0.2f)
        {
            if (_spriteRenderer == null) return;
            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
                _spriteRenderer.color = _baseColor;
            }
            _flashRoutine = StartCoroutine(Flash(duration));
        }

        private IEnumerator Flash(float duration)
        {
            _spriteRenderer.color = _hurtColor;
            yield return new WaitForSeconds(duration);
            _spriteRenderer.color = _baseColor;
            _flashRoutine = null;
        }

        private void OnDisable()
        {
            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
                _flashRoutine = null;
            }
            if (_spriteRenderer != null) _spriteRenderer.color = _baseColor;
        }
    }
}
