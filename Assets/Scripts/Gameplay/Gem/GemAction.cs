using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Bejeweled
{
    /// <summary>
    /// Describe the action the gem will take. Similar to the concept of command
    /// in command buffers in a graphics render pipeline.
    /// 
    /// Used for gem movement and visual effects. The action queue will be constructed
    /// when the level map is updating while the real changes of the actions will take
    /// effect later.
    /// </summary>
    public struct GemAction
    {
        /// <summary>
        /// Type of the gem action.
        /// </summary>
        public GemActionType type;

        /// <summary>
        /// Coordinate of the gem to execute action.
        /// </summary>
        public Vector2Int coord;

        /// <summary>
        /// Destination of the gem.
        /// Only for Move/Swap action.
        /// </summary>
        public Vector2Int destCoord;

        /// <summary>
        /// Type of the gem.
        /// Only for Create/Transform action.
        /// </summary>
        public GemType gemType;

        /// <summary>
        /// Score of the gem.
        /// Only for Clear/Transform action.
        /// </summary>
        public int score;

        /// <summary>
        /// Delay time of the action to be executed.
        /// </summary>
        public float delay;

        #region Helpers

        /// <summary>
        /// Add delay to the action.
        /// </summary>
        public GemAction Delayed(float delay)
        {
            this.delay = delay;
            return this;
        }

        /// <summary>
        /// Create a wait action.
        /// </summary>
        public static GemAction Wait(float delay)
        {
            return new GemAction() {type = GemActionType.None, delay = delay};
        }

        /// <summary>
        /// Create a clear action.
        /// </summary>
        public static GemAction Clear(Vector2Int coord, int score = 0)
        {
            return new GemAction() {type = GemActionType.Clear, coord = coord, score = score};
        }

        /// <summary>
        /// Create a swap action.
        /// </summary>
        public static GemAction Swap(Vector2Int coord1, Vector2Int coord2)
        {
            return new GemAction() {type = GemActionType.Swap, coord = coord1, destCoord = coord2};
        }

        /// <summary>
        /// Create a move action.
        /// </summary>
        public static GemAction Move(Vector2Int coord, Vector2Int destCoord)
        {
            return new GemAction() {type = GemActionType.Move, coord = coord, destCoord = destCoord};
        }

        /// <summary>
        /// Create a create action.
        /// </summary>
        public static GemAction Create(Vector2Int coord, GemType gemType)
        {
            return new GemAction() {type = GemActionType.Create, coord = coord, gemType = gemType};
        }

        /// <summary>
        /// Create a transform action.
        /// </summary>
        public static GemAction Transform(Vector2Int coord, GemType gemType, int score = 0)
        {
            return new GemAction() {type = GemActionType.Transform, coord = coord, gemType = gemType, score = score};
        }

        /// <summary>
        /// Create a barrier action to wait for all the previous actions to finish.
        /// </summary>
        public static GemAction Barrier()
        {
            return new GemAction() {type = GemActionType.Barrier};
        }

        /// <summary>
        /// Create a action queue in a lambda style.
        /// </summary>
        public static List<GemAction> CreateQueue(Action<List<GemAction>> queueOperations)
        {
            var queue = new List<GemAction>();
            queueOperations(queue);
            return queue;
        }

        /// <summary>
        /// Create an empty action queue.
        /// </summary>
        public static List<GemAction> EmptyQueue()
        {
            return new List<GemAction>();
        }

        /// <summary>
        /// Print the action queue for debug purpose.
        /// </summary>
        public static void PrintQueue(List<GemAction> queue)
        {
#if UNITY_EDITOR
            StringBuilder strBuilder = new StringBuilder();

            strBuilder.Append($"ActionQueue Size: {queue.Count}\n");

            strBuilder.Append("[ActionQueue start]\n");

            for (int i = 0; i < queue.Count; ++i)
            {
                var action = queue[i];

                strBuilder.Append($"#{i}: ");

                switch (action.type)
                {
                    case GemActionType.None:
                        strBuilder.Append("None");
                        break;
                    case GemActionType.Clear:
                        strBuilder.Append($"Clear {action.coord} {action.score}");
                        break;
                    case GemActionType.Swap:
                        strBuilder.Append($"Swap {action.coord} {action.destCoord}");
                        break;
                    case GemActionType.Move:
                        strBuilder.Append($"Move {action.coord} {action.destCoord}");
                        break;
                    case GemActionType.Create:
                        strBuilder.Append($"Create {action.coord} {action.gemType.ToString()}");
                        break;
                    case GemActionType.Transform:
                        strBuilder.Append($"Transform {action.coord} {action.gemType.ToString()} {action.score}");
                        break;
                    case GemActionType.Barrier:
                        strBuilder.Append("Barrier");
                        break;
                }

                if (action.delay > 0f)
                {
                    strBuilder.Append($" Delayed {action.delay}s");
                }

                strBuilder.Append('\n');
            }

            strBuilder.Append("[ActionQueue end]\n");

            Debug.Log(strBuilder);
#endif
        }

        #endregion
    }

    public enum GemActionType
    {
        None = 0, // Placeholder, will be ignored when executing
        Clear, // Gem will be cleared by a pair
        Swap, // 2 Gems will be swapped
        Move, // Gem will move to fill in empty slots
        Create, // Gem will be created to fill in empty slots
        Transform, // Gem will be transformed into other types of gem
        Barrier // Wait for all the previous actions to finish
    }
}