using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bejeweled
{
    /// <summary>
    /// Detector for vertical pairs.
    /// </summary>
    public class VerticalPairDetector : IPairDetector
    {
        #region Pair

        /// <summary>
        /// List of matched pairs.
        /// </summary>
        private List<MatchPair> pairList = new List<MatchPair>();

        /// <summary>
        /// Detect matched pairs.
        /// </summary>
        public List<MatchPair> Detect(LevelMap map)
        {
            return DetectRange(map, 0, map.Width, 0, map.Height);
        }

        /// <summary>
        /// Detect pair in a region.
        /// </summary>
        public List<MatchPair> DetectRange(LevelMap map, int xStart, int xEnd, int yStart, int yEnd,
            bool returnIfFindOne = false)
        {
            pairList = new List<MatchPair>();

            for (int i = xStart; i < xEnd; ++i)
            {
                GemType prevGemMatchedType = GemType.None;
                int prevGemCount = 0;
                int prevMagicGemCount = 0;

                for (int j = 0; j < map.Height; ++j)
                {
                    GemType currGemType = map[j, i];

                    // If the current gem is matched with the previous gem
                    GemType matchedGemType = Gem.GetMatchedGemType(currGemType, prevGemMatchedType);
                    if (matchedGemType != GemType.None)
                    {
                        // Update gem type
                        prevGemMatchedType = matchedGemType;

                        // Update gem pair count
                        ++prevGemCount;
                        prevMagicGemCount = (currGemType == GemType.Magic) ? prevMagicGemCount + 1 : 0;

                        continue;
                    }

                    // Add detected pair
                    if (prevGemCount >= 3)
                    {
                        AddNewPair(j - prevGemCount, j, i, prevGemMatchedType);

                        if (returnIfFindOne)
                            return pairList;
                    }

                    // Reset gem stat
                    prevGemMatchedType = currGemType;
                    prevGemCount = prevMagicGemCount + 1;
                    prevMagicGemCount = (currGemType == GemType.Magic) ? prevMagicGemCount + 1 : 0;
                }

                // Add detected pair
                if (prevGemCount >= 3)
                {
                    AddNewPair(map.Height - prevGemCount, map.Height, i, prevGemMatchedType);

                    if (returnIfFindOne)
                        return pairList;
                }
            }

            return pairList;
        }

        /// <summary>
        /// Added new pair to the result from [xBegin, xEnd).
        /// </summary>
        private void AddNewPair(int yBegin, int yEnd, int x, GemType type)
        {
            MatchPair pair;

            pair.type = MatchType.Vertical;
            // Randomly assign the type if it's the pair of all the MAGIC gems that match any types
            pair.gemType = type == GemType.Magic ? GemType.Crystal : type;
            pair.coords = new Vector2Int[Mathf.Max(yEnd - yBegin, 0)];

            for (int i = 0, y = yBegin; y < yEnd; ++i, ++y)
            {
                pair.coords[i] = new Vector2Int(x, y);
            }

            pairList.Add(pair);
        }

        #endregion

        #region Swap

        /// <summary>
        /// Detect possible swaps.
        /// </summary>
        public MatchSwap? DetectSwap(LevelMap map)
        {
            for (int i = 1; i < map.Height - 1; ++i)
            {
                for (int j = 0; j < map.Width; ++j)
                {
                    // Get gem type of neighbour gems.
                    GemType self  = map[i, j];
                    GemType left  = (j > 0)             ? map[i, j - 1] : GemType.None;
                    GemType right = (j < map.Width - 1) ? map[i, j + 1] : GemType.None;
                    GemType up    = map[i - 1, j];
                    GemType down  = map[i + 1, j];

                    bool upDownMatched = Gem.IsMatched(up, down);
                    bool upSelfMatched = Gem.IsMatched(up, self);
                    bool downSelfMatched = Gem.IsMatched(down, self);

                    // up X O X down
                    if (upDownMatched && !upSelfMatched)
                    {
                        GemType matchedType = Gem.GetMatchedGemType(up, down);
                        if (Gem.IsMatched(matchedType, left))
                        {
                            return CreateMatchSwap(j, i, j - 1, i, matchedType);
                        }

                        if (Gem.IsMatched(matchedType, right))
                        {
                            return CreateMatchSwap(j, i, j + 1, i, matchedType);
                        }
                    }

                    // Magic Gem may apply to both cases below
                    // so we have to check both cases separately

                    // up X X O down
                    if (upSelfMatched)
                    {
                        GemType matchedType = Gem.GetMatchedGemType(up, self);
                        MatchSwap? swap = DetectGemSwap(map, j, i + 1, matchedType, new Vector2Int(0, -1));
                        if (swap.HasValue)
                            return swap;
                    }

                    // up O X X down
                    if (downSelfMatched)
                    {
                        GemType matchedType = Gem.GetMatchedGemType(down, self);
                        MatchSwap? swap = DetectGemSwap(map, j, i - 1, matchedType, new Vector2Int(0, 1));
                        if (swap.HasValue)
                            return swap;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Find a gem match in the neighbour gems except the given mate cell direction.
        /// </summary>
        private MatchSwap? DetectGemSwap(LevelMap map, int x, int y, GemType type, Vector2Int mateDir)
        {
            Vector2Int[] neighbourCoords = map.GetNeighbourCoords(new Vector2Int(x, y));
            Vector2Int mateCoord = new Vector2Int(x, y) + mateDir;

            foreach (var coord in neighbourCoords)
            {
                if (coord == mateCoord)
                    continue;

                if (Gem.IsMatched(type, map[coord]))
                    return CreateMatchSwap(x, y, coord.x, coord.y, type);
            }

            return null;
        }

        /// <summary>
        /// Create a match swap.
        /// </summary>
        private MatchSwap CreateMatchSwap(int x1, int y1, int x2, int y2, GemType gemType)
        {
            MatchSwap swap;

            swap.coords = new Tuple<Vector2Int, Vector2Int>(new Vector2Int(x1, y1), new Vector2Int(x2, y2));
            swap.gemType = gemType;

            return swap;
        }

        #endregion
    }
}