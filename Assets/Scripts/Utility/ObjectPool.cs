using System.Collections.Generic;
using UnityEngine;

namespace Bejeweled.Utility
{
    /// <summary>
    /// Simple object pool.
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        #region Properties

        /// <summary>
        /// Prefab of the object.
        /// </summary>
        [SerializeField]
        public GameObject objectPrefab;

        /// <summary>
        /// Initialized pool size.
        /// </summary>
        [SerializeField]
        public int initPoolSize = 0;

        /// <summary>
        /// Number of objects to create for each warm process.
        /// </summary>
        [SerializeField]
        public int warmObjectCount = 5;

        /// <summary>
        /// Stored object list.
        /// </summary>
        protected List<GameObject> objectList = new List<GameObject>();

        /// <summary>
        /// Size of the pool.
        /// </summary>
        public int PoolSize => objectList.Count;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialization.
        /// </summary>
        protected void Start()
        {
            WarmPool(initPoolSize);
        }

        /// <summary>
        /// Warm the pool with the given size.
        /// </summary>
        protected void WarmPool(int newPoolSize)
        {
            int objectCountToAdd = Mathf.Max(0, newPoolSize - PoolSize);

            for (int i = 0; i < objectCountToAdd; ++i)
            {
                GameObject poolObject = GameObject.Instantiate(objectPrefab);
                poolObject.transform.parent = transform;
                poolObject.transform.localPosition = Vector3.zero;
                poolObject.transform.localRotation = Quaternion.identity;

                poolObject.SetActive(false);
                objectList.Add(poolObject);
            }
        }

        #endregion

        #region Pool Operations

        /// <summary>
        /// Shrink pool size.
        /// </summary>
        public void Shrink(int newPoolSize)
        {
            if (PoolSize <= newPoolSize)
            {
                return;
            }

            List<GameObject> removedObjects = new List<GameObject>();
            removedObjects = objectList.GetRange(newPoolSize, PoolSize - newPoolSize);
            objectList.RemoveRange(newPoolSize, PoolSize - newPoolSize);

            foreach (var poolObject in removedObjects)
            {
                Destroy(poolObject);
            }
        }

        /// <summary>
        /// Reserve pool size.
        /// </summary>
        public void Reserve(int newPoolSize)
        {
            if (PoolSize >= newPoolSize)
            {
                return;
            }

            WarmPool(newPoolSize);
        }

        /// <summary>
        /// Clear the pool.
        /// </summary>
        public void Clear()
        {
            Shrink(0);
        }

        #endregion

        #region Factory Methods

        /// <summary>
        /// Acquire a new instance from the pool.
        /// </summary>
        public GameObject Acquire()
        {
            if (PoolSize <= 0)
            {
                WarmPool(warmObjectCount);
            }

            // Get latest added object to make use of cache locality
            GameObject poolObject = objectList[PoolSize - 1];
            objectList.RemoveAt(PoolSize - 1);

            poolObject.transform.parent = transform.root;
            poolObject.transform.localPosition = Vector3.zero;
            poolObject.transform.localRotation = Quaternion.identity;

            poolObject.SetActive(true);

            return poolObject;
        }

        /// <summary>
        /// Release an existing instance back to the pool.
        /// </summary>
        public void Release(GameObject poolObject)
        {
            poolObject.transform.parent = transform;
            poolObject.transform.localPosition = Vector3.zero;
            poolObject.transform.localRotation = Quaternion.identity;

            poolObject.SetActive(false);
            objectList.Add(poolObject);
        }

        #endregion
    }
}