using TMPro;
using UnityEngine;

namespace Bejeweled.UI
{
    /// <summary>
    /// Timer UI controller.
    /// </summary>
    public sealed class UITimer : UIBaseBinder
    {
        /// <summary>
        /// Timer text.
        /// </summary>
        [SerializeField]
        [Space]
        private TextMeshProUGUI timerText;

        /// <summary>
        /// Initialization.
        /// </summary>
        private void Start()
        {
            Debug.Assert(timerText, "Timer text is not set!");

            BindBlackboard<int>("timer", OnUpdateTimer);
        }

        /// <summary>
        /// Update the timer on change.
        /// </summary>
        private void OnUpdateTimer(int timer)
        {
            int minute = Mathf.FloorToInt(timer / 60.0f);
            int second = Mathf.FloorToInt(timer % 60.0f);

            timerText.text = $"{minute:00}:{second:00}";
            timerText.color = (timer > 0 && timer <= 10) ? Color.red : Color.white;
        }
    }
}