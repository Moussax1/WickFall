using UnityEngine;
using UnityEngine.UI;

namespace WickFall.UI
{
    public class CandleFlameFlicker : MonoBehaviour
    {
        [SerializeField] private float speed = 8f;
        [SerializeField] private float scaleVariation = 0.08f;

        private Image _img;
        private Vector3 _baseScale;

        private void Awake()
        {
            _img = GetComponent<Image>();
            _baseScale = transform.localScale;
        }

        private void Update()
        {
            float noise = Mathf.PerlinNoise(Time.unscaledTime * speed, 0f);
            float scaleMod = 1f + (noise - 0.5f) * 2f * scaleVariation;
            transform.localScale = new Vector3(_baseScale.x * (2f - scaleMod), _baseScale.y * scaleMod, _baseScale.z);
        }
    }
}
