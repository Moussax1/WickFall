using UnityEngine;
using WickFall.Player;

namespace WickFall.CameraControl
{
    /// <summary>
    /// Smooth 2D Top-Down camera follow with target look-ahead and position damping.
    /// Strictly maintains orthographic depth (-10) and follows the Candle Golem.
    /// </summary>
    public class CameraFollow2D : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 0f, -10f);

        [Header("Damping")]
        [SerializeField] private float _smoothTime = 0.2f;
        [SerializeField] private float _lookAheadDistance = 0.6f;

        private Vector3 _velocity = Vector3.zero;
        private PlayerController _playerController;

        private void Start()
        {
            if (_target == null)
            {
                var player = Object.FindFirstObjectByType<PlayerController>();
                if (player != null)
                {
                    _target = player.transform;
                    _playerController = player;
                }
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 targetPos = _target.position + _offset;

            if (_playerController != null && _playerController.IsMoving)
            {
                Vector2 facing = _playerController.FacingVector;
                targetPos += new Vector3(facing.x, facing.y, 0f) * _lookAheadDistance;
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, _smoothTime);
        }

        public void SetTarget(Transform target)
        {
            _target = target;
            if (_target != null)
            {
                _playerController = _target.GetComponent<PlayerController>();
            }
        }
    }
}
