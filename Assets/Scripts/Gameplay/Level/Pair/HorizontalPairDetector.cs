using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bejeweled
{
    /// <summary>
    /// Detector for horizontal pairs.
    /// </summary>
    public class HorizontalPairDetector : IPairDetector
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

            for (int i = yStart; i < yEnd; ++i)
            {
                GemType prevGemMatchedType = GemType.None;
                int prevGemCount = 0;
                int prevMagicGemCount = 0;

                for (int j = 0; j < map.Width; ++j)
                {
                    GemType currGemType = map[i, j];

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
                    AddNewPair(map.Width - prevGemCount, map.Width, i, prevGemMatchedType);

                    if (returnIfFindOne)
                        return pairList;
                }
            }

            return pairList;
        }

        /// <summary>
        /// Added new pair to the result from [xBegin, xEnd).
        /// </summary>
        private void AddNewPair(int xBegin, int xEnd, int y, GemType type)
        {
            MatchPair pair;

            pair.type = MatchType.Horizontal;
            // Randomly assign the type if it's the pair of all the MAGIC gems that match any types
            pair.gemType = type == GemType.Magic ? GemType.Crystal : type;
            pair.coords = new Vector2Int[Mathf.Max(xEnd - xBegin, 0)];

            for (int i = 0, x = xBegin; x < xEnd; ++i, ++x)
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
            for (int i = 0; i < map.Height; ++i)
            {
                for (int j = 1; j < map.Width - 1; ++j)
                {
                    // Get gem type of neighbour gems.
                    GemType self  = map[i, j];
                    GemType left  = map[i, j - 1];
                    GemType right = map[i, j + 1];
                    GemType up    = (i > 0)              ? map[i - 1, j] : GemType.None;
                    GemType down  = (i < map.Height - 1) ? map[i + 1, j] : GemType.None;

                    bool leftRightMatched = Gem.IsMatched(left, right);
                    bool leftSelfMatched = Gem.IsMatched(left, self);
                    bool rightSelfMatched = Gem.IsMatched(right, self);

                    // X O X
                    if (leftRightMatched && !leftSelfMatched)
                    {
                        GemType matchedType = Gem.GetMatchedGemType(left, right);

                        if (Gem.IsMatched(matchedType, up))
                        {
                            return CreateMatchSwap(j, i, j, i - 1, matchedType);
                        }

                        if (Gem.IsMatched(matchedType, down))
                        {
                            return CreateMatchSwap(j, i, j, i + 1, matchedType);
                        }
                    }

                    // Magic Gem may apply to both cases below
                    // so we have to check both cases separately

                    // X X O
                    if (leftSelfMatched)
                    {
                        GemType matchedType = Gem.GetMatchedGemType(left, self);
                        MatchSwap? swap = DetectGemSwap(map, j + 1, i, matchedType, new Vector2Int(-1, 0));
                        if (swap.HasValue)
                            return swap;
                    }

                    // O X X
                    if (rightSelfMatched)
                    {
                        GemType matchedType = Gem.GetMatchedGemType(right, self);
                        MatchSwap? swap = DetectGemSwap(map, j - 1, i, matchedType, new Vector2Int(1, 0));
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