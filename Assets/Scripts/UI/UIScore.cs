using TMPro;
using UnityEngine;

namespace Bejeweled.UI
{
    /// <summary>
    /// Score UI controller.
    /// </summary>
    public sealed class UIScore : UIBaseBinder
    {
        /// <summary>
        /// Score text.
        /// </summary>
        [SerializeField]
        [Space]
        private TextMeshProUGUI scoreText;

        /// <summary>
        /// Initialization.
        /// </summary>
        private void Start()
        {
            Debug.Assert(scoreText, "Score text is not set!");

            BindBlackboard<int>("score", OnUpdateScore);
        }

        /// <summary>
        /// Update score on change.
        /// </summary>
        /// <param name="score"></param>
        private void OnUpdateScore(int score)
        {
            scoreText.text = score.ToString();
        }
    }
}