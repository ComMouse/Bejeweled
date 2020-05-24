using System;
using System.Collections.Generic;
using Bejeweled.Utility;
using UnityEngine;

namespace Bejeweled
{
    /// <summary>
    /// Gem factory.
    /// A factory class to manage gem instances with object pools.
    /// </summary>
    public class GemFactory : SingletonBehaviour<GemFactory>
    {
        #region Properties

        [Serializable]
        public struct GemTypeInfo
        {
            public GemType type;
            public GameObject prefab;
        }

        /// <summary>
        /// Gem type list for editing in Unity editor.
        /// </summary>
        [SerializeField]
        protected List<GemTypeInfo> gemTypeList = new List<GemTypeInfo>();

        /// <summary>
        /// Initial size of each pool.
        /// </summary>
        [SerializeField]
        protected int gemPoolSize = 20;

        /// <summary>
        /// Gem type dict generated from the list above.
        /// </summary>
        protected Dictionary<GemType, GameObject> gemTypeDict = new Dictionary<GemType, GameObject>();

        /// <summary>
        /// Gem pool list to store unused objects.
        /// </summary>
        protected Dictionary<GemType, ObjectPool> gemPoolDict = new Dictionary<GemType, ObjectPool>();

        #endregion

        #region Initialization

        protected void Start()
        {
            InitializeDict();

            InitializeGems();
        }

        protected void InitializeDict()
        {
            foreach (var gemType in gemTypeList) gemTypeDict[gemType.type] = gemType.prefab;
        }

        protected void InitializeGems()
        {
            foreach (var gemType in gemTypeDict)
            {
                var gemPool = gameObject.AddComponent<ObjectPool>();
                gemPool.initPoolSize = gemPoolSize;
                gemPool.objectPrefab = gemType.Value;
                gemPoolDict.Add(gemType.Key, gemPool);
            }
        }

        #endregion

        #region Factory Methods

        public Gem CreateGem(GemType type)
        {
            var gemObject = gemPoolDict[type].Acquire();
            var gem = gemObject.GetComponent<Gem>();

            // Initialize the gem to reset its state
            gem.Initialize();

            return gem;
        }

        public void DestroyGem(Gem gem)
        {
            gemPoolDict[gem.type].Release(gem.gameObject);
        }

        #endregion
    }
}