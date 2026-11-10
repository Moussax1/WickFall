using UnityEngine;
using UnityEngine.UI;

namespace WickFall.UI
{
    /// <summary>
    /// Produces an organic, warm candle flame flickering effect on UI candle flames.
    /// </summary>
    public class CandleFlameFlicker : MonoBehaviour
    {
        [Header("Flicker Parameters")]
        [SerializeField] private float speed = 8f;
        [SerializeField] private float intensityVariation = 0.15f;
        [SerializeField] private float scaleVariation = 0.08f;

        private Image targetImage;
        private Vector3 initialScale;
        private Color initialColor;
        private float noiseOffset;

        private void Awake()
        {
            targetImage = GetComponent<Image>();
            initialScale = transform.localScale;
            if (targetImage != null)
                initialColor = targetImage.color;
            noiseOffset = Random.Range(0f, 100f);
        }

        private void Update()
        {
            float time = Time.unscaledTime * speed + noiseOffset;
            float perlin = Mathf.PerlinNoise(time, 0f);

            // Scale flicker (flame stretching)
            float scaleMod = 1f + (perlin - 0.5f) * 2f * scaleVariation;
            transform.localScale = new Vector3(initialScale.x * (2f - scaleMod), initialScale.y * scaleMod, initialScale.z);

            // Alpha/Color flicker (luminance dancing)
            if (targetImage != null)
            {
                float alphaMod = 1f - (perlin * intensityVariation);
                targetImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, initialColor.a * alphaMod);
            }
        }
    }
}
