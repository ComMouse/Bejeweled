using System.Collections.Generic;
using UnityEngine;

namespace Bejeweled
{
    /// <summary>
    /// Map for gem objects.
    /// This class manages all the gem objects in the scene.
    /// </summary>
    public class GemMap : MonoBehaviour
    {
        #region Properties

        /// <summary>
        /// Gem x axis interval.
        /// </summary>
        [SerializeField]
        private Vector3 gemXInterval = new Vector3(1.0f, 0.0f, 0.0f);

        /// <summary>
        /// Gem y axis interval.
        /// </summary>
        [SerializeField]
        private Vector3 gemYInterval = new Vector3(0.0f, -1.0f, 0.0f);

        /// <summary>
        /// Gem map.
        /// </summary>
        [HideInInspector]
        public Gem[,] map = new Gem[0, 0];

        /// <summary>
        /// Free list to store unused gem objects.
        /// </summary>
        private List<Gem> freeList = new List<Gem>();

        /// <summary>
        /// Root of the free list. Used to store free gem objects.
        /// </summary>
        private Transform freeListRoot;

        /// <summary>
        /// Convenient index operator to get coordinate of the gem.
        /// </summary>
        /// <param name="gem"></param>
        /// <returns></returns>
        public Vector2Int this[Gem gem]
        {
            get => GetGemCoord(gem);
            set => SetGemCoord(gem, value);
        }

        /// <summary>
        /// Convenient index operator to access map data.
        /// </summary>
        public Gem this[int y, int x]
        {
            get => map[y, x];
            set
            {
                if (map[y, x] != null && map[y, x] != value)
                {
                    FreeGem(map[y, x]);
                }

                SetGemCoord(value, new Vector2Int(x, y));
            }
        }

        /// <summary>
        /// Convenient index operator to access map data.
        /// </summary>
        public Gem this[Vector2Int coord]
        {
            get => this[coord.y, coord.x];
            set => this[coord.y, coord.x] = value;
        }

        #endregion

        #region Game Loop

        /// <summary>
        /// Initialization.
        /// </summary>
        private void Start()
        {
            GameManager.Instance.GemMap = this;

            // Create a free list to store unused gems before they are destroyed.
            freeListRoot = new GameObject("FreeList").transform;
            freeListRoot.parent = transform;
            freeListRoot.gameObject.SetActive(false);

            GameManager.Instance.GemDirector?.BindMap(this);
        }

        /// <summary>
        /// Initialize the gems from the level map.
        /// </summary>
        public void CreateFromMap(LevelMap levelMap)
        {
            ClearGems();

            map = new Gem[levelMap.Height, levelMap.Width];

            for (int i = 0; i < levelMap.Height; ++i)
            {
                for (int j = 0; j < levelMap.Width; ++j)
                {
                    GemType gemType = levelMap[i, j];
                    if (gemType != GemType.None)
                    {
                        var gem = GemFactory.Instance.CreateGem(gemType);
                        SetGemCoord(gem, new Vector2Int(j, i));
                    }
                }
            }
        }

        /// <summary>
        /// Clear all the gems in the map.
        /// </summary>
        public void ClearGems()
        {
            if (map == null)
                return;

            foreach (var gem in map)
            {
                GemFactory.Instance.DestroyGem(gem);
            }

            foreach (var gem in freeList)
            {
                GemFactory.Instance.DestroyGem(gem);
            }

            map = new Gem[0, 0];
            freeList.Clear();
        }

        #endregion

        #region Gem Operations

        /// <summary>
        /// Remove a gem from the map and add to freelist.
        /// If the gem needs to be used later it can be taken
        /// back from the freelist.
        /// </summary>
        public void FreeGem(Gem gem)
        {
            gem.transform.parent = freeListRoot;
            freeList.Add(gem);
        }

        /// <summary>
        /// Destroy all the unused gems in the freelist.
        /// </summary>
        public void ClearUnusedGems()
        {
            foreach (var gem in freeList)
            {
                GemFactory.Instance.DestroyGem(gem);
            }

            freeList.Clear();
        }

        /// <summary>
        /// Add a new gem into the map.
        /// </summary>
        public void AddGem(Gem gem, Vector2Int coord, bool moveToCoord = false)
        {
            if (gem == null)
            {
                Debug.LogWarning("Empty gem passed in to set a coordinate");
                return;
            }

            gem.transform.parent = transform;
            if (moveToCoord)
            {
                gem.AppearAt(coord);
            }
            else
            {
                gem.PlaceAt(coord);
            }

            map[coord.y, coord.x] = gem;

            if (!gem.isActiveAndEnabled)
            {
                gem.gameObject.SetActive(true);
            }

            if (freeList.Contains(gem))
            {
                freeList.Remove(gem);
            }
        }

        #endregion

        #region Coordinates

        /// <summary>
        /// Set an existing gem to a new coordinate.
        /// </summary>
        public void SetGemCoord(Gem gem, Vector2Int coord, bool moveToCoord = false)
        {
            if (gem == null)
            {
                Debug.LogWarning("Empty gem passed in to set a coordinate");
                return;
            }

            gem.transform.parent = transform;
            if (moveToCoord)
            {
                gem.MoveTo(coord);
            }
            else
            {
                gem.PlaceAt(coord);
            }

            map[coord.y, coord.x] = gem;

            if (!gem.isActiveAndEnabled)
            {
                gem.gameObject.SetActive(true);
            }

            if (freeList.Contains(gem))
            {
                freeList.Remove(gem);
            }
        }

        /// <summary>
        /// Get the coordinate of the gem by its position.
        /// </summary>
        public Vector2Int GetGemCoord(Gem gem)
        {
            // Not in gem map
            if (gem.transform.parent != transform)
            {
                return new Vector2Int(-1, -1);
            }

            Vector3 pos = gem.transform.localPosition;
            float magnitude = pos.magnitude;

            // Project position vector into x and y unit directions to get x and y coordinate
            float xCoord = Vector3.Dot(pos, gemXInterval) / Mathf.Max(magnitude, Mathf.Epsilon);
            float yCoord = Vector3.Dot(pos, gemYInterval) / Mathf.Max(magnitude, Mathf.Epsilon);

            return new Vector2Int(Mathf.RoundToInt(xCoord), Mathf.RoundToInt(yCoord));
        }

        /// <summary>
        /// Get the local position of a coordinate.
        /// </summary>
        public Vector3 GetCoordPos(Vector2Int coord)
        {
            return GetCoordPos(coord.x, coord.y);
        }

        /// <summary>
        /// Get the local position of a coordinate.
        /// </summary>
        public Vector3 GetCoordPos(int x, int y)
        {
            return gemXInterval * x + gemYInterval * y;
        }

        #endregion
    }
}