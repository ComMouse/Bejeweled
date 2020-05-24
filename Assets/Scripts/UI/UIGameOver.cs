using System.Collections;
using UnityEngine;

namespace Bejeweled.UI
{
    /// <summary>
    /// Game over cover controller.
    /// </summary>
    public sealed class UIGameOver : UIBaseBinder
    {
        /// <summary>
        /// Cover mesh renderer.
        /// </summary>
        [SerializeField]
        [Space]
        private MeshRenderer renderer;

        /// <summary>
        /// Fade appearing speed.
        /// </summary>
        [SerializeField]
        private float fadeSpeed = 5f;

        /// <summary>
        /// Original alpha of the material.
        /// </summary>
        private float originalAlpha = 1.0f;

        /// <summary>
        /// Initialization.
        /// </summary>
        private void Start()
        {
            renderer = GetComponent<MeshRenderer>();
            Debug.Assert(renderer != null);

            originalAlpha = renderer.material.color.a;

            BindBlackboard<bool>("IsGameOver", UpdateGameOverStatus);

            UpdateGameOverStatus(false);
        }

        /// <summary>
        /// Update hover status when game is over.
        /// </summary>
        private void UpdateGameOverStatus(bool isGameOver)
        {
            renderer.enabled = isGameOver;

            if (isGameOver)
            {
                StartCoroutine(FadeInOverlay());
            }
            else
            {
                StopAllCoroutines();
            }
        }

        /// <summary>
        /// Fade in operations.
        /// </summary>
        private IEnumerator FadeInOverlay()
        {
            SetOpaque(0f);

            float pastTime = 0f;
            float fadeTime = 1.0f / Mathf.Max(fadeSpeed, 1e-6f); // Avoid zero speed

            while (true)
            {
                yield return null;

                pastTime += Time.deltaTime;

                SetOpaque(Mathf.SmoothStep(0f, 1f, pastTime / fadeTime));

                if (pastTime > fadeTime)
                {
                    yield break;
                }
            }
        }

        /// <summary>
        /// Set opaque of the material from 0-1.
        /// </summary>
        private void SetOpaque(float value)
        {
            var color = renderer.material.color;
            color.a = Mathf.Clamp01(value) * originalAlpha;
            renderer.material.color = color;
        }
    }
}