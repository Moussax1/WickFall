using System.Collections;
using UnityEngine;
using WickFall.Player;

namespace WickFall.Environment
{
    public enum SpikeState { Retracted, Warning, Extended }

    [RequireComponent(typeof(Collider2D))]
    public class SpikeTrap : MonoBehaviour
    {
        [SerializeField] private float _retractedDuration = 2.0f;
        [SerializeField] private float _warningDuration = 0.6f;
        [SerializeField] private float _extendedDuration = 1.0f;
        [SerializeField] private float _spikeDamage = 25f;
        [SerializeField] private float _knockbackForce = 4.0f;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _safeColor = new Color(0.4f, 0.4f, 0.4f, 0.4f);
        [SerializeField] private Color _warningColor = new Color(0.9f, 0.5f, 0.1f, 0.8f);
        [SerializeField] private Color _lethalColor = new Color(1.0f, 0.2f, 0.15f, 1.0f);

        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioClip _springSound;

        private SpikeState _state = SpikeState.Retracted;
        public SpikeState CurrentState => _state;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start() => StartCoroutine(Cycle());

        private IEnumerator Cycle()
        {
            while (true)
            {
                _state = SpikeState.Retracted;
                if (_spriteRenderer != null) _spriteRenderer.color = _safeColor;
                yield return new WaitForSeconds(_retractedDuration);

                _state = SpikeState.Warning;
                if (_spriteRenderer != null) _spriteRenderer.color = _warningColor;
                yield return new WaitForSeconds(_warningDuration);

                _state = SpikeState.Extended;
                if (_spriteRenderer != null) _spriteRenderer.color = _lethalColor;
                if (_sfxSource != null && _springSound != null) _sfxSource.PlayOneShot(_springSound);
                yield return new WaitForSeconds(_extendedDuration);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (_state != SpikeState.Extended) return;
            if (other.TryGetComponent<PlayerWax>(out var wax))
            {
                Vector2 dir = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;
                wax.TakeDamage(_spikeDamage, dir, _knockbackForce);
            }
        }
    }
}
