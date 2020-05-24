using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bejeweled.UI
{
    /// <summary>
    /// Game button controller.
    /// </summary>
    public sealed class UIGameButton : UIBaseBinder
    {
        /// <summary>
        /// UI button.
        /// </summary>
        [SerializeField]
        [Space]
        private Button button;

        /// <summary>
        /// Button text.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI text;

        /// <summary>
        /// If the button works as a hint button.
        /// </summary>
        private bool isHintButton = false;

        /// <summary>
        /// If the button works as a play button.
        /// </summary>
        private bool isPlayButton = true;

        /// <summary>
        /// Button initialization.
        /// </summary>
        private void Start()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (text == null)
            {
                text = GetComponentInChildren<TextMeshProUGUI>();
            }

            Debug.Assert(button);
            Debug.Assert(text);

            button.onClick.AddListener(OnButtonClick);

            BindBlackboard<bool>("IsPlaying", UpdateHintStatus);
            BindBlackboard<bool>("IsGameOver", UpdateReplayStatus);
        }

        /// <summary>
        /// Button click handler.
        /// </summary>
        private void OnButtonClick()
        {
            if (isPlayButton)
            {
                Blackboard.Set("StartGameTrigger", true);
            }
            else if (isHintButton)
            {
                Blackboard.Set("HintTrigger", true);
            }
        }

        /// <summary>
        /// Update hint button validness.
        /// </summary>
        private void UpdateHintStatus(bool isPlaying)
        {
            if (!isPlaying)
                return;

            text.text = "Hint";
            isHintButton = true;
            isPlayButton = false;
        }

        /// <summary>
        /// Update replay button validness.
        /// </summary>
        private void UpdateReplayStatus(bool isGameOver)
        {
            if (!isGameOver)
                return;

            text.text = "Restart";
            isHintButton = false;
            isPlayButton = true;
        }
    }
}