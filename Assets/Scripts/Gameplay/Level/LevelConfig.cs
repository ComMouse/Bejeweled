using System;
using UnityEngine;

namespace Bejeweled
{
    /// <summary>
    /// Preconfigured level data.
    /// </summary>
    [Serializable]
    public class LevelConfig : ScriptableObject
    {
        /// <summary>
        /// Total time of the level.
        /// </summary>
        public int timer;

        /// <summary>
        /// Allowed gem types in the level.
        /// </summary>
        public GemType[] gemTypes;
    }
}