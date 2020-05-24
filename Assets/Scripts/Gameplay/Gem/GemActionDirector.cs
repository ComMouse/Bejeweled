using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bejeweled
{
    /// <summary>
    /// Director of gem actions.
    /// This class manages execution of gem action queues.
    /// </summary>
    public class GemActionDirector : MonoBehaviour
    {
        #region Properties

        /// <summary>
        /// Gem map shortcut.
        /// </summary>
        private GemMap gemMap;

        /// <summary>
        /// Action queues to be executed.
        /// </summary>
        private Queue<List<GemAction>> pendingQueueList = new Queue<List<GemAction>>();

        /// <summary>
        /// Action queues executing.
        /// The queue will be removed from the dictionary once it's finished.
        /// </summary>
        private Dictionary<int, Coroutine> playingQueueDict = new Dictionary<int, Coroutine>();

        /// <summary>
        /// Get if the director is executing any queues.
        /// </summary>
        public bool IsPlaying => playingQueueDict.Count > 0 || pendingQueueList.Count > 0;

        /// <summary>
        /// Enable/Disable the director. Unfinished queue will remain playing.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        #endregion

        #region Game Loop

        /// <summary>
        /// Initialization.
        /// </summary>
        private void Start()
        {
            GameManager.Instance.GemDirector = this;

            if (gemMap == null)
            {
                gemMap = GameManager.Instance.GemMap;
            }
        }

        /// <summary>
        /// Per frame update.
        /// </summary>
        private void Update()
        {
            if (IsEnabled)
            {
                if (pendingQueueList.Count > 0 && playingQueueDict.Count == 0)
                {
                    var nextQueue = pendingQueueList.Dequeue();
                    ExecuteQueue(nextQueue);
                }
            }
        }

        /// <summary>
        /// Bind a gem map to the director.
        /// </summary>
        public void BindMap(GemMap map)
        {
            this.gemMap = map;
        }

        #endregion

        #region Execution

        /// <summary>
        /// Execute a new action queue.
        /// </summary>
        /// <param name="waitPlayingQueues">
        /// If the queue will be executed after the previous queues are finished.
        /// Switching it to false will let the director play the queue immediately.
        /// </param>
        public void PlayQueue(List<GemAction> queue, bool waitPlayingQueues = true)
        {
            if (queue == null || queue.Count == 0)
                return;

            if (waitPlayingQueues)
            {
                pendingQueueList.Enqueue(queue);
            }
            else
            {
                ExecuteQueue(queue);
            }
        }

        /// <summary>
        /// Stop all the queues.
        /// </summary>
        public void StopAllQueues()
        {
            foreach (var item in playingQueueDict)
            {
                StopCoroutine(item.Value);
            }

            StopAllCoroutines();

            playingQueueDict.Clear();
        }

        /// <summary>
        /// Internal implementation of executing a queue.
        /// It will start a coroutine to handle with the queue.
        /// </summary>
        private void ExecuteQueue(List<GemAction> queue)
        {
            int queueId = NextQueueId();
            var coroutine = StartCoroutine(ExecuteQueueAsync(queueId, queue));
            playingQueueDict.Add(queueId, coroutine);
        }

        /// <summary>
        /// Internal implementation of executing a queue.
        /// </summary>
        private IEnumerator ExecuteQueueAsync(int queueId, List<GemAction> queue)
        {
            // Current barrier state
            GemActionBarrier barrier = new GemActionBarrier();

            // Execute the queue
            foreach (var action in queue)
            {
                if (action.delay > 0f)
                {
                    yield return new WaitForSeconds(action.delay);
                }

                switch (action.type)
                {
                    case GemActionType.None:
                        break;
                    case GemActionType.Barrier:
                        yield return StartCoroutine(barrier.WaitAsync());
                        barrier.Reset();
                        break;
                    default:
                        StartCoroutine(ExecuteActionAsync(action, barrier));
                        break;
                }
            }

            // Wait for all the actions to finish
            yield return StartCoroutine(barrier.WaitAsync());

            // Free unused objects
            gemMap.ClearUnusedGems();

            // Remove the queue from playing dict
            playingQueueDict.Remove(queueId);
        }

        /// <summary>
        /// Execute gem action.
        /// </summary>
        private IEnumerator ExecuteActionAsync(GemAction action, GemActionBarrier barrier)
        {
            barrier.AddTask();

            switch (action.type)
            {
                case GemActionType.Clear:
                {
                    var gem = gemMap[action.coord];
                    gem.Disappear();
                    GameManager.Instance.Level.GainScore(action.score);

                    yield return new WaitWhile(() => gem.IsAnimating);
                    gemMap.FreeGem(gem); // Remove the gem from gem map
                    break;
                }
                case GemActionType.Swap:
                {
                    var gem1 = gemMap[action.coord];
                    var gem2 = gemMap[action.destCoord];

                    // Remove the gem from gem map first to avoid them
                    // being overlapped by each other
                    gemMap.FreeGem(gem1);
                    gemMap.FreeGem(gem2);

                    gemMap.SetGemCoord(gem1, action.destCoord, true);
                    gemMap.SetGemCoord(gem2, action.coord, true);

                    yield return new WaitWhile(() => gem1.IsAnimating || gem2.IsAnimating);
                    break;
                }
                case GemActionType.Move:
                {
                    var gem = gemMap[action.coord];
                    gemMap.SetGemCoord(gem, action.destCoord, true);

                    yield return new WaitWhile(() => gem.IsAnimating);
                    break;
                }
                case GemActionType.Create:
                {
                    var gem = GemFactory.Instance.CreateGem(action.gemType);
                    gemMap.AddGem(gem, action.coord, true);

                    yield return new WaitWhile(() => gem.IsAnimating);
                    break;
                }
                case GemActionType.Transform:
                {
                    var newGem = GemFactory.Instance.CreateGem(action.gemType);
                    // Old gem will be removed from the map automatically
                    gemMap.SetGemCoord(newGem, action.coord);
                    break;
                }
            }

            // Wait for one frame
            yield return null;

            barrier.RemoveTask();
        }

        #endregion

        #region Id Generator

        /// <summary>
        /// Counter for queue id generation.
        /// </summary>
        private static int queueIdGenerator = 0;

        /// <summary>
        /// Generate a unique queue id.
        /// </summary>
        private static int NextQueueId()
        {
            return ++queueIdGenerator;
        }

        #endregion
    }

    /// <summary>
    /// Naive barrier implementation for gem action coroutines.
    /// Expected to work only in single thread cases.
    /// </summary>
    public class GemActionBarrier
    {
        /// <summary>
        /// Count of active tasks.
        /// </summary>
        private int activeTasks = 0;

        /// <summary>
        /// If the barrier can be passed currently.
        /// </summary>
        public bool CanPass => activeTasks <= 0;

        /// <summary>
        /// Add a new task to the barrier.
        /// </summary>
        public void AddTask()
        {
            ++activeTasks;
        }

        /// <summary>
        /// Remove a task from the barrier.
        /// </summary>
        public void RemoveTask()
        {
            --activeTasks;
        }

        /// <summary>
        /// Wait until the barrier can be passed or time runs out.
        /// </summary>
        public IEnumerator WaitAsync(float timeout = 0f)
        {
            float waitExpireTime = timeout > 0f ? Time.realtimeSinceStartup + timeout : float.MaxValue;

            while (true)
            {
                if (CanPass || Time.realtimeSinceStartup > waitExpireTime)
                    yield break;

                yield return null;
            }
        }

        /// <summary>
        /// Reset barrier state.
        /// </summary>
        public void Reset()
        {
            activeTasks = 0;
        }
    }
}