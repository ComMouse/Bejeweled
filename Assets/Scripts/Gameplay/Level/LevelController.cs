using System.Collections;
using Bejeweled.Utility;
using UnityEngine;

namespace Bejeweled
{
    /// <summary>
    /// Level controller.
    /// This class handles core gameplay logic and the game process.
    /// </summary>
    public class LevelController : BaseStateMachineBehaviour<LevelController.LevelState>
    {
        public enum LevelState
        {
            None = 0,
            Prepare = 1,
            Ongoing = 2,
            Finish = 3
        }

        #region Properties

        /// <summary>
        /// Current level config.
        /// </summary>
        public LevelConfig config;

        /// <summary>
        /// Current level data.
        /// </summary>
        [HideInInspector]
        public LevelData data;

        /// <summary>
        /// Current level map.
        /// </summary>
        [HideInInspector]
        public LevelMap map;

        /// <summary>
        /// Current state of the level.
        /// </summary>
        [Header("Debug")]
        [ReadOnlyField]
        [SerializeField]
        protected LevelState currentState = LevelState.None;

        /// <summary>
        /// Current state of the level.
        /// </summary>
        public override LevelState State
        {
            get => currentState;
            protected set => currentState = value;
        }

        /// <summary>
        /// Get if the map is frozen for regeneration.
        /// </summary>
        public bool IsMapFrozen { get; protected set; } = false;

        /// <summary>
        /// Get if the player can swap gems.
        /// </summary>
        public bool CanSwapGem =>
            State == LevelState.Ongoing && (!GameManager.Instance.GemDirector?.IsPlaying ?? false) && !IsMapFrozen;

        #endregion

        #region Game Loop

        /// <summary>
        /// Initialization.
        /// </summary>
        protected void Start()
        {
            GameManager.Instance.Level = this;

            data = new LevelData();
        }

        /// <summary>
        /// Per frame update.
        /// </summary>
        protected void Update()
        {
            UpdateState();
        }

        #endregion

        #region Level Operations

        /// <summary>
        /// Initialize a level.
        /// </summary>
        protected void InitLevel()
        {
            data = new LevelData();
            data.timer = config.timer;
            data.score = 0;

            GameManager.Instance.Blackboard.Set("timer", Mathf.FloorToInt(data.timer));
            GameManager.Instance.Blackboard.Set("score", data.score);
        }

        /// <summary>
        /// Generate a new level.
        /// </summary>
        protected void GenerateLevel()
        {
            map = LevelMap.CreateFromTypes(config.gemTypes);

            GameManager.Instance.GemMap.CreateFromMap(map);

            Debug.Log("Level generated!");
        }

        /// <summary>
        /// Regenerate an existing level.
        /// This will be an asynchronous process.
        /// </summary>
        protected void StartRegenerateLevel()
        {
            StartCoroutine(RegenerateMapAsync());
        }

        /// <summary>
        /// Implementation of regenerating an existing level.
        /// </summary>
        protected IEnumerator RegenerateMapAsync()
        {
            yield return new WaitWhile(() => GameManager.Instance.GemDirector.IsPlaying);

            GameManager.Instance.GemDirector.StopAllQueues();
            GameManager.Instance.Gem.StopDrag();
            GameManager.Instance.GemMap.ClearGems();

            GenerateLevel();

            Debug.Log("Level regenerated due to no pair found");
        }

        /// <summary>
        /// Start a new level.
        /// </summary>
        public void StartLevel()
        {
            if (State != LevelState.Prepare && State != LevelState.Finish)
                return;

            TransitState(LevelState.Ongoing);
        }

        /// <summary>
        /// Update the current ongoing level.
        /// </summary>
        protected void UpdateLevel()
        {
            data.timer = Mathf.Max(0f, data.timer - Time.deltaTime);
            GameManager.Instance.Blackboard.Set("timer", Mathf.CeilToInt(data.timer));

            if (data.timer <= 0)
            {
                TransitState(LevelState.Finish);
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                if (!CanSwapGem)
                    return;

                // Detect the hint swap
                MatchSwap? swap = map.DetectMatchedSwap();
                if (swap == null)
                    return;

                TrySwap(swap.Value.coords.Item1, swap.Value.coords.Item2);
            }
        }

        /// <summary>
        /// Finish a level.
        /// </summary>
        protected IEnumerator FinishLevelAsync()
        {
            IsMapFrozen = true;

            yield return new WaitWhile(() => GameManager.Instance.GemDirector.IsPlaying);

            yield return new WaitForSeconds(0.5f);

            GameManager.Instance.Blackboard.Set("IsGameOver", true);
            GameManager.Instance.Blackboard.AddListener("StartGameTrigger", OnTriggerStartGame);

            IsMapFrozen = false;
        }

        /// <summary>
        /// Clear the current level.
        /// </summary>
        public void ClearLevel()
        {
            GameManager.Instance.GemDirector.StopAllQueues();

            GameManager.Instance.GemMap.ClearGems();

            InitLevel();
        }

        #endregion

        #region Game FSM

        /// <summary>
        /// Update game according to the current state.
        /// </summary>
        protected void UpdateState()
        {
            switch (State)
            {
                case LevelState.None:
                    TransitState(LevelState.Prepare);
                    break;
                case LevelState.Prepare:
                    break;
                case LevelState.Ongoing:
                    UpdateLevel();
                    break;
                case LevelState.Finish:
                    break;
            }
        }

        /// <summary>
        /// Handle state exit logic.
        /// </summary>
        protected override void PreTransitState(LevelState oldState, LevelState newState)
        {
            switch (oldState)
            {
                case LevelState.Prepare:
                    GameManager.Instance.Blackboard.RemoveListener("StartGameTrigger", OnTriggerStartGame);
                    break;
                case LevelState.Ongoing:
                    GameManager.Instance.Blackboard.Set("IsPlaying", false);
                    GameManager.Instance.Blackboard.RemoveListener("HintTrigger", OnTriggerHint);
                    break;
                case LevelState.Finish:
                    ClearLevel();
                    GameManager.Instance.Blackboard.RemoveListener("StartGameTrigger", OnTriggerStartGame);
                    GameManager.Instance.Blackboard.Set("IsGameOver", false);
                    break;
            }
        }

        /// <summary>
        /// Handle state enter logic.
        /// </summary>
        protected override void PostTransitState(LevelState oldState, LevelState newState)
        {
            switch (newState)
            {
                case LevelState.Prepare:
                    InitLevel();
                    GameManager.Instance.Blackboard.AddListener("StartGameTrigger", OnTriggerStartGame);
                    break;
                case LevelState.Ongoing:
                    GenerateLevel();
                    GameManager.Instance.Blackboard.AddListener("HintTrigger", OnTriggerHint);
                    GameManager.Instance.Blackboard.Set("IsPlaying", true);
                    break;
                case LevelState.Finish:
                    StartCoroutine(FinishLevelAsync());
                    break;
            }
        }

        #endregion

        #region Map Operations

        /// <summary>
        /// Clear all the existing pairs.
        /// 
        /// Gem changes will happen later with an action queue.
        /// </summary>
        protected void ClearPairs()
        {
            int combo = 1;
            float scoreMultiplier = 1f;

            var actionQueue = GemAction.EmptyQueue();

            // Detect pairs
            var matchedPairs = map.DetectMatchedPairs();

            // Clear-Move-Fill-Detect Loop until no matches detected
            do
            {
                // Clear
                float pairMultiplier = scoreMultiplier * (1 + Mathf.Max(0, matchedPairs.Count - 1));
                actionQueue.AddRange(map.ClearPairs(
                    matchedPairs,
                    pairMultiplier,
                    LevelConstant.GEM_MULTIPLIER,
                    LevelConstant.MAGIC_MULTIPLIER));
                actionQueue.Add(GemAction.Wait(LevelConstant.CLEAR_WAIT_TIME));

                // Move & Fill
                var moveGemQueue = map.MoveGems();
                var fillGemQueue = map.FillGems();

                // Add move/fill actions
                if (moveGemQueue.Count + fillGemQueue.Count > 0)
                {
                    actionQueue.AddRange(moveGemQueue);
                    actionQueue.Add(GemAction.Barrier());
                    actionQueue.AddRange(fillGemQueue);
                    actionQueue.Add(GemAction.Barrier());
                    actionQueue.Add(GemAction.Wait(LevelConstant.MOVE_WAIT_TIME));
                }

                // Detect more pairs
                matchedPairs = map.DetectMatchedPairs();
                ++combo;
                scoreMultiplier *= LevelConstant.COMBO_MULTIPLIER;
            } while (matchedPairs.Count > 0);

            GemAction.PrintQueue(actionQueue);

            GameManager.Instance.GemDirector.PlayQueue(actionQueue);
        }

        /// <summary>
        /// Check if there is any possible swap in the map.
        /// </summary>
        public bool CheckSwap()
        {
            MatchSwap? swap = map.DetectMatchedSwap();

            if (swap != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try swapping two gems and clear the pairs.
        /// If failed, the gems will be swapped back.
        /// 
        /// Gem changes will happen later with an action queue.
        /// </summary>
        public void TrySwap(Vector2Int coord1, Vector2Int coord2)
        {
            var actionQueue = GemAction.EmptyQueue();

            var gemQueue = map.TrySwap(coord1, coord2);

            // If no pair found after swap, play a fake swap
            if (gemQueue.Count == 0)
            {
                actionQueue.Add(GemAction.Swap(coord1, coord2));
                actionQueue.Add(GemAction.Barrier().Delayed(LevelConstant.SWAP_FAIL_WAIT_TIME));
                actionQueue.Add(GemAction.Swap(coord1, coord2));

                GameManager.Instance.GemDirector.PlayQueue(actionQueue);
                return;
            }

            // Add swap action and execute
            actionQueue.Add(GemAction.Swap(coord1, coord2));
            actionQueue.Add(GemAction.Barrier().Delayed(LevelConstant.SWAP_WAIT_TIME));

            GameManager.Instance.GemDirector.PlayQueue(actionQueue);

            // Clear pairs after swapping
            ClearPairs();

            // Regenerate level if no swap detected
            if (!CheckSwap())
            {
                StartRegenerateLevel();
            }
        }

        #endregion

        #region Score

        /// <summary>
        /// Gain score from gems.
        /// </summary>
        /// <param name="score"></param>
        public void GainScore(int score)
        {
            data.score += score;
            GameManager.Instance.Blackboard.Set("score", data.score);
        }

        #endregion

        #region Trigger Handlers

        /// <summary>
        /// Handles game start trigger.
        /// </summary>
        protected void OnTriggerStartGame(string key, object value, DataBlackboard blackboard)
        {
            StartLevel();

            // Set trigger back to false
            blackboard.Set(key, false, true);
        }

        /// <summary>
        /// Handles hint trigger.
        /// </summary>
        protected void OnTriggerHint(string key, object value, DataBlackboard blackboard)
        {
            // Set trigger back to false
            blackboard.Set(key, false, true);

            if (!CanSwapGem)
                return;

            // Detect the hint swap
            MatchSwap? swap = map.DetectMatchedSwap();
            if (swap == null)
                return;

            Gem gem1 = GameManager.Instance.GemMap[swap.Value.coords.Item1];
            Gem gem2 = GameManager.Instance.GemMap[swap.Value.coords.Item2];

            // Select the hint gem
            if (Gem.IsMatched(swap.Value.gemType, gem1.type))
                GameManager.Instance.Gem.Select(gem1);
            else
                GameManager.Instance.Gem.Select(gem2);
        }

        #endregion
    }
}