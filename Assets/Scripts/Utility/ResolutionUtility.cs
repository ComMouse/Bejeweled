using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bejeweled.Utility
{
    /// <summary>
    /// Utility to adjust screen resolution
    /// </summary>
    public static class ResolutionUtility
    {
        /// <summary>
        /// Set game screen to a window.
        /// </summary>
        public static void ScaleToWindow()
        {
            Vector2Int nativeResolution = GetSystemResolution();

            if (nativeResolution.y < 920)
            {
                Screen.SetResolution(1024, 768, FullScreenMode.Windowed);
            }
            else
            {
                Screen.SetResolution(1280, 960, FullScreenMode.Windowed);
            }
        }

        /// <summary>
        /// Set game screen to fullscreen.
        /// </summary>
        public static void ScaleToFullScreen()
        {
            Vector2Int nativeResolution = GetSystemResolution();

            int width = 0, height = 0;

            if (nativeResolution.x > nativeResolution.y)
            {
                width = nativeResolution.y / 3 * 4;
                height = nativeResolution.y;
            }
            else
            {
                width = nativeResolution.x;
                height = nativeResolution.x / 4 * 3;
            }
            

            Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
        }

        /// <summary>
        /// Get system resolution of the main display.
        /// </summary>
        public static Vector2Int GetSystemResolution()
        {
            return new Vector2Int(Display.main.systemWidth, Display.main.systemHeight);
        }
    }
}
