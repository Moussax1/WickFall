using UnityEngine;
using WickFall.Player;

namespace WickFall.Items
{
    /// <summary>
    /// Interactive wax shard pickup that restores the player's wax/health pool and light radius.
    /// Features a gentle hovering sine bob and collectible audio cue.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class WaxPickup : MonoBehaviour
    {
        [Header("Pickup Value")]
        [SerializeField] private float _waxAmount = 20f;
        [SerializeField] private bool _isCluster = false;

        [Header("Animation")]
        [SerializeField] private float _bobSpeed = 3.5f;
        [SerializeField] private float _bobHeight = 0.08f;

        [Header("Audio")]
        [SerializeField] private AudioClip _pickupSound;

        private Vector3 _initialLocalPosition;
        private bool _isCollected;

        public float WaxAmount => _waxAmount;
        public bool IsCluster => _isCluster;

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
            _initialLocalPosition = transform.localPosition;
        }

        private void Update()
        {
            if (_isCollected) return;

            // Subtle vertical float
            float offset = Mathf.Sin(Time.time * _bobSpeed + transform.position.x) * _bobHeight;
            transform.localPosition = _initialLocalPosition + new Vector3(0f, offset, 0f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isCollected) return;

            if (other.TryGetComponent<PlayerWax>(out var playerWax))
            {
                if (playerWax.IsDead) return;

                _isCollected = true;
                playerWax.RestoreWax(_waxAmount);

                if (_pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(_pickupSound, transform.position, 0.8f);
                }

                Destroy(gameObject);
            }
        }
    }
}
