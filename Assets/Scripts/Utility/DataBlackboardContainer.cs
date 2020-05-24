using UnityEngine;

namespace Bejeweled.Utility
{
    /// <summary>
    /// Blackboard container to hold a blackboard instance.
    /// </summary>
    public class DataBlackboardContainer : MonoBehaviour
    {
        #region Properties

        /// <summary>
        /// If the blackboard will be set the default one.
        /// </summary>
        [SerializeField]
        protected bool setAsDefault;

        /// <summary>
        /// If the container will be kept over the scenes.
        /// </summary>
        [SerializeField]
        protected bool dontDestroyOnLoad;

        /// <summary>
        /// Get the blackboard.
        /// </summary>
        public DataBlackboard Blackboard { get; protected set; }

        /// <summary>
        /// Get the default blackboard container.
        /// </summary>
        public static DataBlackboardContainer Default { get; set; }

        #endregion

        #region Game Loop

        /// <summary>
        /// Initialization.
        /// </summary>
        protected void Awake()
        {
            if (setAsDefault)
            {
                Default = this;
            }

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            Blackboard = new DataBlackboard();
        }

        /// <summary>
        /// Destruction.
        /// </summary>
        protected void OnDestroy()
        {
            // Clear default reference
            if (Default == this)
            {
                Default = null;
            }
        }

        #endregion
    }
}