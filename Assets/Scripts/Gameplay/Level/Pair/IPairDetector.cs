using System.Collections.Generic;

namespace Bejeweled
{
    /// <summary>
    /// Interface for gem pair detectors.
    /// </summary>
    public interface IPairDetector
    {
        /// <summary>
        /// Detect all the existing pairs.
        /// </summary>
        List<MatchPair> Detect(LevelMap map);

        /// <summary>
        /// Detect the existing pairs only considering the changed area given.
        /// </summary>
        List<MatchPair> DetectRange(LevelMap map, int xStart, int xEnd, int yStart, int yEnd,
            bool returnIfFindOne = false);

        /// <summary>
        /// Detect one swap to form a pair. Used for hints and no-pair detection.
        /// </summary>
        MatchSwap? DetectSwap(LevelMap map);
    }
}