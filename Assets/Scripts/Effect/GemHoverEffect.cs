using UnityEngine;

namespace Bejeweled
{
    /// <summary>
    /// Gem hover effect to show draggable areas.
    /// </summary>
    public class GemHoverEffect : MonoBehaviour
    {
        /// <summary>
        /// Debounce time before hover shows.
        /// </summary>
        public float debounceTime = 0.5f;

        /// <summary>
        /// Hover display time.
        /// </summary>
        public float hoverTime = 0.3f;

        /// <summary>
        /// Time waited for hover display.
        /// </summary>
        private float waitTime = 0f;

        /// <summary>
        /// Alpha of the hover material.
        /// </summary>
        private float hoverAlpha = 1.0f;

        /// <summary>
        /// Renderer of the effect.
        /// </summary>
        private MeshRenderer renderer;

        /// <summary>
        /// Material of the effect.
        /// </summary>
        private Material material;

        /// <summary>
        /// Initialization.
        /// </summary>
        private void Awake()
        {
            renderer = GetComponent<MeshRenderer>();
            material = renderer.material;

            hoverAlpha = material.color.a;
        }

        /// <summary>
        /// Activate hover effect.
        /// </summary>
        private void OnEnable()
        {
            SetHover(0f);
            waitTime = 0f;
        }

        /// <summary>
        /// Deactivate hover effect.
        /// </summary>
        private void OnDisable()
        {
            SetHover(0f);
        }

        /// <summary>
        /// Per frame update.
        /// </summary>
        private void Update()
        {
            waitTime += Time.deltaTime;

            if (waitTime < debounceTime) return;

            SetHover(Mathf.Clamp01(Mathf.InverseLerp(debounceTime, debounceTime + hoverTime, waitTime)));
        }

        /// <summary>
        /// Update hover value.
        /// </summary>
        private void SetHover(float value)
        {
            var alpha = Mathf.Clamp01(value) * hoverAlpha;
            var color = material.color;
            if (Mathf.Abs(color.a - alpha) < 1e-3f)
                return;

            color.a = alpha;
            material.color = color;
        }
    }
}