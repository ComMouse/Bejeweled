using System;
using UnityEngine;

namespace Bejeweled.Utility
{
    /// <summary>
    /// Resolution controller to provide in-game resolution change.
    /// </summary>
    public class ResolutionController : MonoBehaviour
    {
        /// <summary>
        /// Is the left alt key pressed.
        /// </summary>
        private bool isLeftAltKeyDown = false;

        /// <summary>
        /// Is the right alt key pressed.
        /// </summary>
        private bool isRightAltKeyDown = false;

        /// <summary>
        /// Initialization.
        /// </summary>
        private void Start()
        {
            // Set to window on startup.
            ResolutionUtility.ScaleToWindow();
        }

        /// <summary>
        /// Per frame update.
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt))
                isLeftAltKeyDown = true;

            if (Input.GetKeyDown(KeyCode.RightAlt))
                isRightAltKeyDown = true;

            if (Input.GetKeyUp(KeyCode.LeftAlt))
                isLeftAltKeyDown = true;

            if (Input.GetKeyUp(KeyCode.RightAlt))
                isRightAltKeyDown = true;

            if ((isLeftAltKeyDown || isRightAltKeyDown) &&
                Input.GetKeyDown(KeyCode.Return))
            {
                if (Screen.fullScreen)
                    ResolutionUtility.ScaleToWindow();
                else
                    ResolutionUtility.ScaleToFullScreen();
            }
        }
    }
}
