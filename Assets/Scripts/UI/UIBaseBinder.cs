using System;
using System.Collections.Generic;
using Bejeweled.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace Bejeweled.UI
{
    using DataChangeAction = UnityAction<string, object, DataBlackboard>;

    /// <summary>
    /// Base binding class for UI controllers.
    /// This class provides convenient methods to register blackboard listeners.
    /// </summary>
    public abstract class UIBaseBinder : MonoBehaviour
    {
        public class UIBindListener : Tuple<string, DataChangeAction>
        {
            public UIBindListener(string item1, DataChangeAction item2) : base(item1, item2)
            {
            }
        }

        /// <summary>
        /// Blackboard to bind data listeners. Leave it empty to use the default Blackboard.
        /// </summary>
        [SerializeField]
        [Tooltip("Blackboard to bind data listeners. Leave it empty to use the default Blackboard.")]
        protected DataBlackboardContainer blackboardContainer;

        /// <summary>
        /// List of registered listeners to automatically destroy them.
        /// </summary>
        protected List<UIBindListener> registeredListeners = new List<UIBindListener>();

        /// <summary>
        /// Get the blackboard instance.
        /// </summary>
        protected DataBlackboard Blackboard =>
            blackboardContainer?.Blackboard ?? GetDefaultBlackboard();

        /// <summary>
        /// Initialization.
        /// </summary>
        protected virtual void Awake()
        {
            // Nothing here. Just to align with OnDestroy() below
            // in case someone forgets to execute base.OnDestroy().
        }

        /// <summary>
        /// Destruction.
        /// </summary>
        protected virtual void OnDestroy()
        {
            foreach (var (key, callback) in registeredListeners)
            {
                Blackboard?.RemoveListener(key, callback);
            }
        }

        /// <summary>
        /// Register a key binding callback.
        /// </summary>
        protected void BindBlackboard<ValueType>(string key, Action<ValueType> onUpdateCallback)
        {
            if (Blackboard == null)
                Debug.LogError("Failed to find a valid Blackboard to bind key!");

            DataChangeAction onChangeAction = (k, v, b) => onUpdateCallback(b.Get<ValueType>(k));
            Blackboard.AddListener(key, onChangeAction);

            registeredListeners.Add(new UIBindListener(key, onChangeAction));
        }

        /// <summary>
        /// Register a group of keys binding callback.
        /// </summary>
        protected void BindBlackboard<ValueType>(string[] keys, Action<string, ValueType> onUpdateCallback)
        {
            if (Blackboard == null)
                Debug.LogError("Failed to find a valid Blackboard to bind keys!");

            DataChangeAction onChangeAction = (k, v, b) => onUpdateCallback(k, b.Get<ValueType>(k));
            Blackboard.AddListener(keys, onChangeAction);

            foreach (var key in keys)
            {
                registeredListeners.Add(new UIBindListener(key, onChangeAction));
            }
        }

        /// <summary>
        /// Get the default blackboard instance.
        /// </summary>
        protected static DataBlackboard GetDefaultBlackboard()
        {
            return DataBlackboardContainer.Default?.Blackboard;
        }
    }
}