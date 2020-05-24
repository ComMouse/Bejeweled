using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Bejeweled.Utility
{
    // Alias for the action
    using DataChangeAction = UnityAction<string, object, DataBlackboard>;

    // Event on data changing
    public class DataChangeEvent : UnityEvent<string, object, DataBlackboard>
    {
    }

    /// <summary>
    /// Blackboard to set data and monitor data changes.
    /// </summary>
    public class DataBlackboard
    {
        #region Properties

        /// <summary>
        /// Dictionary to store data.
        /// </summary>
        protected Dictionary<string, object> blackboardDict;

        /// <summary>
        /// Event callbacks registered to data.
        /// </summary>
        protected Dictionary<string, DataChangeEvent> blackboardEvents;

        #endregion

        #region Life Cycle

        /// <summary>
        /// Constructor.
        /// </summary>
        public DataBlackboard()
        {
            blackboardDict = new Dictionary<string, object>();
            blackboardEvents = new Dictionary<string, DataChangeEvent>();
        }

        /// <summary>
        /// Clear the blackboard.
        /// </summary>
        public void Clear()
        {
            blackboardDict.Clear();
            blackboardEvents.Clear();
        }

        #endregion

        #region Listeners

        /// <summary>
        /// Register a listener to a key.
        /// </summary>
        public void AddListener(string key, DataChangeAction eventCallback)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key", "Invalid key name to register callback!");
            }

            if (eventCallback == null)
            {
                return;
            }

            if (!blackboardEvents.ContainsKey(key))
            {
                blackboardEvents[key] = new DataChangeEvent();
            }

            blackboardEvents[key].AddListener(eventCallback);
        }

        /// <summary>
        /// Register a listener to a group of keys.
        /// </summary>
        public void AddListener(IEnumerable<string> keys, DataChangeAction eventCallback)
        {
            foreach (var key in keys)
            {
                AddListener(key, eventCallback);
            }
        }

        /// <summary>
        /// Remove a listener from a key.
        /// </summary>
        public void RemoveListener(string key, DataChangeAction eventCallback)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key", "Invalid key name to remove callback!");
            }

            if (eventCallback == null)
            {
                return;
            }

            if (!blackboardEvents.ContainsKey(key))
            {
                return;
            }

            blackboardEvents[key].RemoveListener(eventCallback);
        }

        /// <summary>
        /// Remove a listener from a group of key.
        /// </summary>
        public void RemoveListener(IEnumerable<string> keys, DataChangeAction eventCallback)
        {
            foreach (var key in keys)
            {
                RemoveListener(key, eventCallback);
            }
        }

        /// <summary>
        /// Invoke listeners of a key.
        /// </summary>
        protected virtual void InvokeListeners(string key, object oldValue, object newValue)
        {
            if (!blackboardEvents.ContainsKey(key))
            {
                return;
            }

            blackboardEvents[key].Invoke(key, newValue, this);
        }

        #endregion

        #region Data Operations

        /// <summary>
        /// Notify change of one variable if
        /// it's referenced and changed by external code.
        /// </summary>
        public void NotifyChange(string key)
        {
            if (!blackboardDict.ContainsKey(key))
            {
                return;
            }

            ValueType value = Get<ValueType>(key);

            InvokeListeners(key, value, value);
        }

        /// <summary>
        /// Get the value of a key.
        /// </summary>
        public ValueType Get<ValueType>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return default(ValueType);
            }

            object value;
            bool valueFetched = blackboardDict.TryGetValue(key, out value);
            if (valueFetched)
            {
                return value is ValueType type ? type : default;
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Get the char value of a key.
        /// </summary>
        public char GetChar(string key)
        {
            return (char) GetObject(key);
        }

        /// <summary>
        /// Get the boolean value of a key.
        /// </summary>
        public bool GetBool(string key)
        {
            return (bool) GetObject(key);
        }

        /// <summary>
        /// Get the int value of a key.
        /// </summary>
        public int GetInt(string key)
        {
            return (int) GetObject(key);
        }

        /// <summary>
        /// Get the long value of a key.
        /// </summary>
        public long GetLong(string key)
        {
            return (long) GetObject(key);
        }

        /// <summary>
        /// Get the float value of a key.
        /// </summary>
        public float GetFloat(string key)
        {
            return (float) GetObject(key);
        }

        /// <summary>
        /// Get the string value of a key.
        /// </summary>
        public string GetString(string key)
        {
            return (string) GetObject(key);
        }

        /// <summary>
        /// Get the object value of a key.
        /// </summary>
        public object GetObject(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return default(ValueType);
            }

            blackboardDict.TryGetValue(key, out var value);
            return value;
        }

        /// <summary>
        /// Set the value of a key.
        /// </summary>
        public void Set<ValueType>(string key, ValueType value, bool skipCallback = false)
        {
            ValueType oldValue = default;

            if (blackboardDict.ContainsKey(key))
            {
                oldValue = Get<ValueType>(key);

                if (oldValue is IComparable comparable &&
                    comparable.CompareTo(value) == 0)
                {
                    return;
                }
            }

            blackboardDict[key] = value;

            if (!skipCallback)
            {
                InvokeListeners(key, oldValue, value);
            }
        }

        /// <summary>
        /// Index access operator for convenience.
        /// </summary>
        public object this[string key]
        {
            get => GetObject(key);
            set => Set(key, value);
        }

        #endregion
    }
}