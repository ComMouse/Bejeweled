using UnityEngine;

namespace Bejeweled
{
    /// <summary>
    /// Gem behaviour controller.
    /// This class handles player's interaction with gems.
    /// </summary>
    public class GemController : MonoBehaviour
    {
        #region Properties

        /// <summary>
        /// Current gem to drag.
        /// </summary>
        private Gem currentDragGem;

        /// <summary>
        /// Current gem hovered.
        /// </summary>
        private Gem currentHoverGem;

        /// <summary>
        /// Last selected gem.
        /// </summary>
        private Gem lastSelectGem;

        /// <summary>
        /// The time that the last gem was selected.
        /// Used to check if the selection is valid.
        /// </summary>
        private float lastSelectTime = 0f;

        /// <summary>
        /// Get if the player is dragging a gem.
        /// </summary>
        public bool IsDragging => currentDragGem;

        #endregion

        #region Game Loop

        /// <summary>
        /// Initialization.
        /// </summary>
        private void Start()
        {
            GameManager.Instance.Gem = this;
        }

        /// <summary>
        /// Per frame update.
        /// </summary>
        private void Update()
        {
            if (IsDragging)
            {
                if (Input.GetMouseButtonUp(0) || !GameManager.Instance.Level.CanSwapGem)
                    StopDrag();
            }
            else
            {
                if (Input.GetMouseButtonDown(0) && currentHoverGem && GameManager.Instance.Level.CanSwapGem)
                    StartDrag(currentHoverGem);
            }
        }

        #endregion

        #region Swap Interaction

        /// <summary>
        /// Start to drag a gem.
        /// </summary>
        public void StartDrag(Gem gem)
        {
            if (currentDragGem != null)
                return;

            // If the game state does not allow dragging
            if (!GameManager.Instance.Level.CanSwapGem)
                return;

            // If the player selected another gem and started to drag this gem,
            // see it as triggering a swap
            if (lastSelectGem != null &&
                Time.time - lastSelectTime < LevelConstant.GEM_SELCT_VALID_TIME &&
                CanSwap(gem, lastSelectGem))
            {
                var selectGem = lastSelectGem;
                ClearSelect();
                GameManager.Instance.Level.TrySwap(gem.coord, selectGem.coord);
                return;
            }

            // Mark the gem as selected
            Select(gem);

            currentDragGem = gem;

            // Show hover of neighbour gems
            Vector2Int[] coords = GameManager.Instance.Level.map.GetNeighbourCoords(gem.coord);
            foreach (var coord in coords)
            {
                GameManager.Instance.GemMap[coord].ShowHover();
            }
        }

        /// <summary>
        /// Stop to drag a gem.
        /// </summary>
        public void StopDrag()
        {
            if (currentDragGem == null)
                return;

            // Hide hover of neighbour gems
            Vector2Int[] coords = GameManager.Instance.Level.map.GetNeighbourCoords(currentDragGem.coord);
            foreach (var coord in coords)
            {
                GameManager.Instance.GemMap[coord].HideHover();
            }

            currentDragGem = null;
        }

        /// <summary>
        /// Select a gem to swap.
        /// </summary>
        public void Select(Gem gem)
        {
            if (gem == null)
                return;

            if (lastSelectGem != null)
                lastSelectGem.HideSelected();

            lastSelectGem = gem;
            lastSelectTime = Time.time;
            lastSelectGem.ShowSelected();
        }

        /// <summary>
        /// Clear gem selection.
        /// </summary>
        public void ClearSelect()
        {
            if (lastSelectGem != null)
                lastSelectGem.HideSelected();

            lastSelectGem = null;
        }

        /// <summary>
        /// Handle when a gem is hovered by the player.
        /// </summary>
        public void OnDragHover(Gem gem)
        {
            currentHoverGem = gem;

            if (IsDragging)
            {
                if (CanSwap(gem, currentDragGem))
                {
                    var dragGem = currentDragGem;
                    StopDrag();
                    ClearSelect();
                    GameManager.Instance.Level.TrySwap(gem.coord, dragGem.coord);
                }
            }
        }

        /// <summary>
        /// Handle when a gem is hovered out by the player.
        /// </summary>
        public void OnDragOut(Gem gem)
        {
            if (currentHoverGem == gem)
                currentHoverGem = null;
        }

        /// <summary>
        /// Helper method to check if two gems can be swapped.
        /// </summary>
        public bool CanSwap(Gem gem1, Gem gem2)
        {
            return Mathf.Abs(gem1.coord.x - gem2.coord.x) + Mathf.Abs(gem1.coord.y - gem2.coord.y) == 1;
        }

        #endregion
    }
}