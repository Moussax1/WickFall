using System;
using System.Collections;
using UnityEngine;

namespace WickFall.Player
{
    public class PlayerDeathSequence : MonoBehaviour
    {
        [SerializeField] private Sprite[] _meltFrames;
        [SerializeField] private AudioClip _extinguishPuffSound;

        private SpriteRenderer _sr;
        private PlayerWax _playerWax;
        private bool _isDying;

        public static event Action OnDeathSequenceFinished;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _playerWax = GetComponent<PlayerWax>();
        }

        private void OnEnable()
        {
            if (_playerWax != null) _playerWax.OnPlayerExtinguished += StartDeathSequence;
        }

        private void OnDisable()
        {
            if (_playerWax != null) _playerWax.OnPlayerExtinguished -= StartDeathSequence;
        }

        public void StartDeathSequence()
        {
            if (_isDying) return;
            _isDying = true;

            var controller = GetComponent<PlayerController>();
            if (controller != null) controller.enabled = false;

            StartCoroutine(MeltRoutine());
        }

        private IEnumerator MeltRoutine()
        {
            if (_extinguishPuffSound != null)
                AudioSource.PlayClipAtPoint(_extinguishPuffSound, transform.position);

            if (_meltFrames != null)
            {
                for (int i = 0; i < _meltFrames.Length; i++)
                {
                    if (_meltFrames[i] != null && _sr != null) _sr.sprite = _meltFrames[i];
                    yield return new WaitForSeconds(0.25f);
                }
            }

            yield return new WaitForSeconds(0.5f);

            OnDeathSequenceFinished?.Invoke();

            var hud = UnityEngine.Object.FindFirstObjectByType<WickFall.UI.GameHUDManager>();
            if (hud != null) hud.ShowGameOver();
        }
    }
}
