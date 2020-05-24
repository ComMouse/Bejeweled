using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bejeweled
{
    /// <summary>
    /// Level gem map.
    /// This class is the data representation of all the gems.
    /// </summary>
    public class LevelMap
    {
        #region Properties

        /// <summary>
        /// Gem map data.
        /// </summary>
        public GemType[,] gemMap;

        /// <summary>
        /// Available gem types in this map.
        /// </summary>
        public GemType[] gemTypes = new GemType[0];

        /// <summary>
        /// Width of the map.
        /// </summary>
        public int Width => LevelConstant.MAP_WIDTH;

        /// <summary>
        /// Height of the map.
        /// </summary>
        public int Height => LevelConstant.MAP_HEIGHT;

        /// <summary>
        /// Convenient index operator to access map data.
        /// </summary>
        public GemType this[int y, int x]
        {
            get => gemMap[y, x];
            set => gemMap[y, x] = value;
        }

        /// <summary>
        /// Convenient index operator to access map data by coord.
        /// </summary>
        public GemType this[Vector2Int coord]
        {
            get => gemMap[coord.y, coord.x];
            set => gemMap[coord.y, coord.x] = value;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Internal constructor.
        /// </summary>
        protected LevelMap()
        {
            gemMap = new GemType[Height, Width];
        }

        #endregion

        #region Gem Generation

        /// <summary>
        /// Fill the empty slots will gems.
        /// </summary>
        public List<GemAction> FillGems(bool isInit = false)
        {
            int gemTypeCount = gemTypes.Length;
            List<GemAction> queue = GemAction.EmptyQueue();

            float fillStartTime = Time.realtimeSinceStartup;

            // The map is split into two halves in a checkerboard style to match the gem
            // generation algorithm below so that a matched pair can be mostly avoided
            // among the newly generated gems.

            // First half of the map
            for (int i = 0; i < Height; ++i)
            {
                for (int j = i % 2; j < Width; j += 2)
                {
                    // Skip existing gems
                    if (gemMap[i, j] != GemType.None)
                        continue;

                    var gemType = GenerateGemType(j, i);
                    gemMap[i, j] = gemType;

                    queue.Add(GemAction.Create(new Vector2Int(j, i), gemType));
                }
            }

            // Second half of the map
            for (int i = 0; i < Height; ++i)
            {
                for (int j = (i + 1) % 2; j < Width; j += 2)
                {
                    // Skip existing gems
                    if (gemMap[i, j] != GemType.None)
                        continue;

                    var gemType = GenerateGemType(j, i);
                    gemMap[i, j] = gemType;
                    queue.Add(GemAction.Create(new Vector2Int(j, i), gemType));
                }
            }

            // 10 Refill Tries will be enough for almost all the cases
            const int maxRefillTries = 10;
            int refilledGemCount = 0;

            // Only refill on initialization
            if (isInit)
            {
                for (int i = 0; i < maxRefillTries; ++i)
                {
                    var matchedPairList = DetectMatchedPairs();

                    if (matchedPairList.Count == 0)
                        break;

                    foreach (var pair in matchedPairList)
                    {
                        var centerGemCoord = pair.coords[pair.coords.Length / 2];
                        this[centerGemCoord] = GenerateGemType(centerGemCoord);
                        ++refilledGemCount;
                    }
                }
            }

            float fillEndTime = Time.realtimeSinceStartup;

            if (isInit)
            {
                Debug.Log(
                    $"Gem fill finished with {refilledGemCount} gems refilled in {(fillEndTime - fillStartTime) * 1000:0.00}ms.");
            }

            return queue;
        }

        /// <summary>
        /// Generate gem type for a coordinate to avoid creating a new pair.
        /// This is the overload version for coordinate parameter.
        /// </summary>
        protected GemType GenerateGemType(Vector2Int coord)
        {
            return GenerateGemType(coord.x, coord.y);
        }

        /// <summary>
        /// Generate gem type for a coordinate to avoid new pairs.
        /// </summary>
        protected GemType GenerateGemType(int x, int y)
        {
            GemType gemLeft  = (x > 0)          ? this[y, x - 1] : GemType.None;
            GemType gemRight = (x < Width - 1)  ? this[y, x + 1] : GemType.None;
            GemType gemUp    = (y > 0)          ? this[y - 1, x] : GemType.None;
            GemType gemDown  = (y < Height - 1) ? this[y + 1, x] : GemType.None;

            List<GemType> typeList = new List<GemType>(gemTypes);

            // Remove the gem if left and right remains the same
            GemType leftRightMatchedType = Gem.GetMatchedGemType(gemLeft, gemRight);
            if (leftRightMatchedType != GemType.None)
                typeList.Remove(leftRightMatchedType);

            // Remove the gem if up and down remains the same
            GemType upDownMatchedType = Gem.GetMatchedGemType(gemUp, gemDown);
            if (upDownMatchedType != GemType.None)
                typeList.Remove(upDownMatchedType);

            // Randomly pick one gem if all the available gem types are removed by the rules above
            if (typeList.Count == 0)
            {
                return gemTypes[UnityEngine.Random.Range(0, gemTypes.Length)];
            }

            return typeList[UnityEngine.Random.Range(0, typeList.Count)];
        }

        #endregion

        #region Gem Operations

        /// <summary>
        /// Try swapping the gem.
        /// An empty queue will be returned if swap fails.
        /// </summary>
        public List<GemAction> TrySwap(Vector2Int coord1, Vector2Int coord2)
        {
            GemType tempGemType = this[coord1];
            this[coord1] = this[coord2];
            this[coord2] = tempGemType;

            // Swap back if no pair exists after the gems swapped
            if (!HasMatchedPair())
            {
                tempGemType = this[coord1];
                this[coord1] = this[coord2];
                this[coord2] = tempGemType;

                return GemAction.EmptyQueue();
            }

            return GemAction.CreateQueue(queue => { queue.Add(GemAction.Swap(coord1, coord2)); });
        }

        /// <summary>
        /// Clear the pairs given and calculate the scores.
        /// </summary>
        public List<GemAction> ClearPairs(
            List<MatchPair> pairs,
            float scoreMultiplier = 1f, // Base score multiplier
            float gemMultiplier = 0f, // Additional gem per pair multiplier
            float magicMultiplier = 0f) // Magic gem multiplier
        {
            Dictionary<Vector2Int, int> clearedGemCountDict = new Dictionary<Vector2Int, int>();
            Dictionary<Vector2Int, float> clearedGemScoreDict = new Dictionary<Vector2Int, float>();
            List<GemAction> queue = GemAction.EmptyQueue();

            foreach (var pair in pairs)
            {
                int pairGems = pair.coords.Length;

                foreach (var coord in pair.coords)
                {
                    if (!clearedGemCountDict.ContainsKey(coord))
                    {
                        clearedGemCountDict[coord] = 0;
                        clearedGemScoreDict[coord] = 0f;

                        if (this[coord] == GemType.Magic)
                        {
                            clearedGemScoreDict[coord] += magicMultiplier;
                        }

                        this[coord] = GemType.None;
                    }

                    clearedGemCountDict[coord]++;
                    clearedGemScoreDict[coord] += 1f + Mathf.Max(0, pairGems - 3) * gemMultiplier;
                }

                // Add a magic gem if the count of gem in a pair is more than 5
                if (pairGems >= 5)
                {
                    clearedGemCountDict[pair.coords[(pairGems - 1) / 2]]++;
                }
            }

            // Add clear action and calculate scores
            foreach (var item in clearedGemScoreDict)
            {
                int score = Mathf.RoundToInt(LevelConstant.BASE_SCORE * item.Value * scoreMultiplier);
                queue.Add(GemAction.Clear(item.Key, score));
            }

            // Add magic gems to gem that are paired twice or more
            bool isMagicGemTransformed = false;
            foreach (var item in clearedGemCountDict)
            {
                // When a gem is cleared more than once a magic gem will be transformed
                if (item.Value > 1)
                {
                    if (!isMagicGemTransformed)
                    {
                        isMagicGemTransformed = true;
                        queue.Add(GemAction.Barrier());
                    }

                    queue.Add(GemAction.Create(item.Key, GemType.Magic));
                    this[item.Key] = GemType.Magic;
                }
            }

            return queue;
        }

        /// <summary>
        /// Move the gems to fill in empty slots.
        /// </summary>
        public List<GemAction> MoveGems()
        {
            List<GemAction> queue = GemAction.EmptyQueue();

            // Trail map storing old indices of gems to track gem movement
            int[,] gemTrailMap = new int[Height, Width];

            // Mark each gem by indices
            for (int i = 0; i < Height; ++i)
            {
                for (int j = 0; j < Width; ++j)
                {
                    if (this[i, j] != GemType.None)
                        gemTrailMap[i, j] = i * Height + j + 1;
                }
            }

            // Move gems by iteration
            for (int iterateTime = 0; iterateTime < Height - 1; ++iterateTime)
            {
                int movedGemCount = 0;

                for (int i = Height - 1; i >= iterateTime + 1; --i)
                {
                    for (int j = 0; j < Width; ++j)
                    {
                        if (gemTrailMap[i, j] != 0)
                            continue;

                        gemTrailMap[i, j] = gemTrailMap[i - 1, j];
                        gemTrailMap[i - 1, j] = 0;
                        ++movedGemCount;
                    }
                }

                if (movedGemCount == 0)
                    break;
            }

            // Detect moved gems and add to queue
            // Iterate from bottom to top to ensure the correct action order
            for (int i = Height - 1; i >= 0; --i)
            {
                for (int j = 0; j < Width; ++j)
                {
                    if (gemTrailMap[i, j] == 0)
                        continue;

                    int gemOldIndex = gemTrailMap[i, j] - 1;
                    int gemCurrIndex = i * Height + j;

                    if (gemOldIndex == gemCurrIndex)
                        continue;

                    int gemOldX = gemOldIndex % Width;
                    int gemOldY = gemOldIndex / Width;

                    queue.Add(GemAction.Move(new Vector2Int(gemOldX, gemOldY), new Vector2Int(j, i)));
                }
            }

            // Execute movement in the order of queue actions
            foreach (var moveAction in queue)
            {
                this[moveAction.destCoord] = this[moveAction.coord];
                this[moveAction.coord] = GemType.None;
            }

            return queue;
        }

        #endregion

        #region Pair Detection

        /// <summary>
        /// Detect if there is any pair in the current map.
        /// </summary>
        public bool HasMatchedPair()
        {
            foreach (var detector in GetPairDetectors())
            {
                if (detector.DetectRange(this, 0, Width, 0, Height, true).Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Detect all the matched pairs in the current map.
        /// </summary>
        public List<MatchPair> DetectMatchedPairs()
        {
            List<MatchPair> pairList = new List<MatchPair>();

            foreach (var detector in GetPairDetectors())
            {
                pairList.AddRange(detector.Detect(this));
            }

            return pairList;
        }

        /// <summary>
        /// Detect any available swap in the current map.
        /// </summary>
        public MatchSwap? DetectMatchedSwap()
        {
            foreach (var detector in GetPairDetectors())
            {
                var swap = detector.DetectSwap(this);
                if (swap.HasValue)
                {
                    return swap;
                }
            }

            return null;
        }

        /// <summary>
        /// Get available pair detectors.
        /// </summary>
        private IPairDetector[] GetPairDetectors()
        {
            return new IPairDetector[]
            {
                new HorizontalPairDetector(),
                new VerticalPairDetector(),
            };
        }

        #endregion

        #region Coordinate Helpers

        /// <summary>
        /// Get neighbour coordinates.
        /// </summary>
        public Vector2Int[] GetNeighbourCoords(Vector2Int coord)
        {
            List<Vector2Int> neighbours = new List<Vector2Int>();

            var leftCoord = new Vector2Int(coord.x - 1, coord.y);
            if (IsValidCoord(leftCoord))
                neighbours.Add(leftCoord);

            var rightCoord = new Vector2Int(coord.x + 1, coord.y);
            if (IsValidCoord(rightCoord))
                neighbours.Add(rightCoord);

            var upCoord = new Vector2Int(coord.x, coord.y - 1);
            if (IsValidCoord(upCoord))
                neighbours.Add(upCoord);

            var downCoord = new Vector2Int(coord.x, coord.y + 1);
            if (IsValidCoord(downCoord))
                neighbours.Add(downCoord);

            return neighbours.ToArray();
        }

        /// <summary>
        /// Check if a coordinate is valid.
        /// </summary>
        public bool IsValidCoord(Vector2Int coord)
        {
            return IsValidCoord(coord.x, coord.y);
        }

        /// <summary>
        /// Check if a coordinate is valid.
        /// </summary>
        public bool IsValidCoord(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        #endregion

        #region Create Methods

        /// <summary>
        /// Initialize all the gems in the slots.
        /// </summary>
        public void InitializeGems()
        {
            FillGems(true);
        }

        /// <summary>
        /// Generate a new empty level.
        /// </summary>
        public static LevelMap CreateEmpty()
        {
            LevelMap map = new LevelMap();

            return map;
        }

        /// <summary>
        /// Generate a new level from gem types.
        /// </summary>
        public static LevelMap CreateFromTypes(GemType[] types)
        {
            LevelMap map = new LevelMap {gemTypes = types};

            map.InitializeGems();

            return map;
        }

        /// <summary>
        /// Generate a new level from level config.
        /// </summary>
        public static LevelMap CreateFromConfig(LevelConfig config)
        {
            LevelMap map = new LevelMap {gemTypes = config.gemTypes};

            map.FillGems(true);

            return map;
        }

        #endregion
    }

    /// <summary>
    /// Matched pair type.
    /// </summary>
    public enum MatchType
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2
    }

    /// <summary>
    /// Matched pair result.
    /// </summary>
    public struct MatchPair
    {
        public MatchType type;
        public GemType gemType;
        public Vector2Int[] coords;
    }

    /// <summary>
    /// Matched swap result.
    /// </summary>
    public struct MatchSwap
    {
        public Tuple<Vector2Int, Vector2Int> coords;
        public GemType gemType;
    }
}