using UnityEngine;
using WickFall.Player;

namespace WickFall.Environment
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class DarknessShroud : MonoBehaviour
    {
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private Vector3 _wickOffset = new Vector3(0f, 0.35f, 0f);
        [SerializeField] private float _maxScale = 1.0f;
        [SerializeField] private float _minScale = 0.38f;
        [SerializeField] private float _lerpSpeed = 3.5f;
        [SerializeField] private float _flickerSpeed = 10f;
        [SerializeField] private float _flickerMagnitude = 0.05f;

        private PlayerWax _playerWax;
        private bool _isCleansed;
        private float _currentScale = 1.0f;

        private void Start()
        {
            if (_playerTransform == null)
            {
                var p = Object.FindFirstObjectByType<PlayerController>();
                if (p != null) { _playerTransform = p.transform; _playerWax = p.GetComponent<PlayerWax>(); }
            }
            if (_playerWax != null) _currentScale = Mathf.Lerp(_minScale, _maxScale, _playerWax.WaxRatio);
            Brazier.OnBrazierKindled += HandleKindled;
        }

        private void OnDestroy() => Brazier.OnBrazierKindled -= HandleKindled;

        private void HandleKindled()
        {
            _isCleansed = true;
            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_isCleansed || _playerTransform == null) return;

            Vector3 pos = _playerTransform.position + _wickOffset;
            pos.z = -1f;
            transform.position = pos;

            float ratio = _playerWax != null ? _playerWax.WaxRatio : 1f;
            float targetScale = Mathf.Lerp(_minScale, _maxScale, ratio);
            _currentScale = Mathf.MoveTowards(_currentScale, targetScale, _lerpSpeed * Time.deltaTime);
            transform.localScale = new Vector3(_currentScale, _currentScale, 1f);
        }
    }
}
