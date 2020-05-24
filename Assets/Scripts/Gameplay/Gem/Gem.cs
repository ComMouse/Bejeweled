using Bejeweled.Utility;
using UnityEngine;

namespace Bejeweled
{
    public enum GemType
    {
        None = 0,
        Diamond = 1,
        Crystal = 2,
        Sapphire = 3,
        Ruby = 4,
        Amethyst = 5,
        Aqua = 6,
        Magic = 99 // Matches any type of gem
    }

    /// <summary>
    /// Gem object class.
    /// This is the gem displayed in the scene and this class
    /// mainly handles effect and display logic.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Gem : BaseStateMachineBehaviour<Gem.GemState>
    {
        public enum GemState
        {
            None = 0,
            Moving = 1,
            Appearing = 2,
            Disappearing = 3,
            Disappeared = 4,
        }

        #region Properties

        /// <summary>
        /// The object of the hover effect.
        /// </summary>
        [SerializeField]
        private GameObject hoverEffect;

        /// <summary>
        /// The object of the select effect.
        /// </summary>
        [SerializeField]
        private GameObject selectEffect;

        /// <summary>
        /// Type of the gem. Set in prefab by default.
        /// </summary>
        [SerializeField]
        public GemType type;

        /// <summary>
        /// Speed of the movement.
        /// </summary>
        [SerializeField]
        public float moveSpeed = 3.0f;

        /// <summary>
        /// Speed of the appear scaling.
        /// </summary>
        [SerializeField]
        public float scaleSpeed = 5.0f;

        /// <summary>
        /// Speed of the disappear scaling.
        /// </summary>
        [SerializeField]
        public float disappearSpeed = 10.0f;

        /// <summary>
        /// Angular speed of the disappear rotating.
        /// </summary>
        [SerializeField]
        public float disappearRotateSpeed = 60.0f;

        /// <summary>
        /// The coordinate of the current gem.
        /// </summary>
        [Header("Debug")]
        [ReadOnlyField]
        public Vector2Int coord;

        /// <summary>
        /// The original scale of the gem for scaling animation.
        /// </summary>
        private Vector3 originalScale;

        /// <summary>
        /// The collider of the gem.
        /// </summary>
        private Collider collider;

        /// <summary>
        /// Get if the gem is animating.
        /// </summary>
        public bool IsAnimating => State != GemState.None && State != GemState.Disappeared;

        /// <summary>
        /// Get if the gem is displayed as selected.
        /// </summary>
        public bool IsSelected => hoverEffect.activeSelf;

        /// <summary>
        /// Get the current animation state of the gem.
        /// </summary>
        public override GemState State { get; protected set; } = GemState.None;

        #endregion

        #region Game Loop

        /// <summary>
        /// Object initialization.
        /// </summary>
        private void Awake()
        {
            originalScale = transform.localScale;
            collider = GetComponent<Collider>();

            Debug.Assert(hoverEffect != null);
            Debug.Assert(selectEffect != null);

            Initialize();
        }

        /// <summary>
        /// Gem initialization on creating from the object pool.
        /// </summary>
        public void Initialize()
        {
            TransitState(GemState.None);

            collider.enabled = true;

            transform.localScale = originalScale;
            transform.localRotation = Quaternion.identity;

            hoverEffect.gameObject.SetActive(false);
            selectEffect.gameObject.SetActive(false);
        }

        /// <summary>
        /// Per frame update.
        /// </summary>
        private void Update()
        {
            UpdateAnimationState();
        }

        #endregion

        #region Behaviours

        /// <summary>
        /// Move the gem to a given coordinate.
        /// </summary>
        public void MoveTo(Vector2Int newCoord)
        {
            coord = newCoord;
            TransitState(GemState.Moving);
        }

        /// <summary>
        /// Let the gem appear at a given coordinate.
        /// </summary>
        public void AppearAt(Vector2Int newCoord)
        {
            PlaceAt(newCoord);
            TransitState(GemState.Appearing);
        }

        /// <summary>
        /// Place the gem at a given coordinate.
        /// Note that no animation will be trigger in this method.
        /// </summary>
        public void PlaceAt(Vector2Int newCoord)
        {
            coord = newCoord;
            transform.localPosition = GameManager.Instance.GemMap.GetCoordPos(coord);
            transform.localScale = originalScale;
        }

        /// <summary>
        /// Let the gem disappear.
        /// </summary>
        public void Disappear()
        {
            if (State == GemState.Disappeared)
                return;

            TransitState(GemState.Disappearing);
        }

        #endregion

        #region Animation FSM

        /// <summary>
        /// Update animation per frame.
        /// </summary>
        protected void UpdateAnimationState()
        {
            switch (State)
            {
                case GemState.Moving:
                {
                    var destPos = GameManager.Instance.GemMap.GetCoordPos(coord);
                    transform.localPosition =
                        Vector3.MoveTowards(transform.localPosition, destPos, moveSpeed * Time.deltaTime);

                    if (Vector3.Distance(transform.localPosition, destPos) < 1e-3f) TransitState(GemState.None);

                    break;
                }
                case GemState.Appearing:
                {
                    transform.localScale =
                        Vector3.MoveTowards(transform.localScale, originalScale, scaleSpeed * Time.deltaTime);

                    if (Vector3.Distance(transform.localScale, originalScale) < 1e-3f) TransitState(GemState.None);

                    break;
                }
                case GemState.Disappearing:
                {
                    transform.localScale =
                        Vector3.MoveTowards(transform.localScale, Vector3.zero, disappearSpeed * Time.deltaTime);

                    transform.Rotate(0f, disappearRotateSpeed * Time.deltaTime, 0f);

                    if (Vector3.Distance(transform.localScale, Vector3.zero) < 1e-3f) TransitState(GemState.Disappeared);

                    break;
                }
            }
        }

        /// <summary>
        /// Clear the animation state before switching to a new state.
        /// </summary>
        protected override void PreTransitState(GemState oldState, GemState newState)
        {
            switch (oldState)
            {
                case GemState.Moving:
                    transform.localPosition = GameManager.Instance.GemMap.GetCoordPos(coord);
                    transform.localScale = originalScale;
                    break;
                case GemState.Appearing:
                    transform.localScale = originalScale;
                    break;
                case GemState.Disappearing:
                    transform.localScale = originalScale;
                    transform.localRotation = Quaternion.identity;
                    break;
                case GemState.Disappeared:
                    transform.localScale = originalScale;
                    break;
            }
        }

        /// <summary>
        /// Prepare for the animation state after switching to a new state.
        /// </summary>
        protected override void PostTransitState(GemState oldState, GemState newState)
        {
            switch (newState)
            {
                case GemState.Moving:
                    break;
                case GemState.Appearing:
                    transform.localScale = Vector3.zero;
                    break;
                case GemState.Disappearing:
                    break;
                case GemState.Disappeared:
                    transform.localScale = Vector3.zero;
                    break;
            }
        }

        #endregion

        #region Effect & Mouse Events

        /// <summary>
        /// Show select effect.
        /// </summary>
        public void ShowSelected()
        {
            selectEffect.SetActive(true);
        }

        /// <summary>
        /// Hide select effect.
        /// </summary>
        public void HideSelected()
        {
            selectEffect.SetActive(false);
        }

        /// <summary>
        /// Show hover effect.
        /// </summary>
        public void ShowHover()
        {
            hoverEffect.SetActive(true);
        }

        /// <summary>
        /// Hide hover effect.
        /// </summary>
        public void HideHover()
        {
            hoverEffect.SetActive(false);
        }

        /// <summary>
        /// Handles mouse hover event.
        /// </summary>
        private void OnMouseOver()
        {
            GameManager.Instance.Gem.OnDragHover(this);
        }

        /// <summary>
        /// Handles mouse hover off event.
        /// </summary>
        private void OnMouseExit()
        {
            GameManager.Instance.Gem.OnDragOut(this);
        }

        #endregion

        #region Match Utlitities

        /// <summary>
        /// Check if two types of gem can be matched into a pair.
        /// </summary>
        public static bool IsMatched(GemType gem1, GemType gem2)
        {
            if (gem1 == GemType.Magic || gem2 == GemType.Magic)
                return true;

            if (gem1 == GemType.None || gem2 == GemType.None)
                return false;

            return gem1 == gem2;
        }

        /// <summary>
        /// Get the gem type if two types of gem can be matched into a pair.
        /// </summary>
        /// <returns>GemType.None if the gems are not matched.</returns>
        public static GemType GetMatchedGemType(GemType gem1, GemType gem2)
        {
            if (!IsMatched(gem1, gem2))
                return GemType.None;

            if (gem1 == GemType.Magic)
                return gem2;

            if (gem2 == GemType.Magic)
                return gem1;

            return gem1;
        }

        #endregion
    }
}