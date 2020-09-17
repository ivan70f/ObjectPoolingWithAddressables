using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ObjectPool
{
    /// <summary>
    /// Data for pooled object.
    /// </summary>
    [Serializable]
    public struct PoolObjectData
    {
        /// <summary>
        /// Reference to prefab.
        /// </summary>
        [SerializeField] private AssetReference prefab;
        
        /// <summary>
        /// Required amount of instances that will not be destroyed by objects collector.
        /// </summary>
        [SerializeField] private int maxInstancesAmount;

        [SerializeField] private bool instantiateObjectOnAwake;
        
        /// <summary>
        /// If count of instantiated objects is greater than instancesAmount parameter you could destoy them on delay
        /// to free the memory. Pool objects will be destroyed when they are returned to pool.
        /// </summary>
        [SerializeField] private bool destroyOverflowInstances;

        /// <summary>
        /// Delay to destroy overflow instances.
        /// </summary>
        [SerializeField] private float overflowInstancesLifeTime;

        /// <summary>
        /// Count of currently instantiated objects.
        /// </summary>
        [SerializeField] private int instancesCount;

        private bool registerCreated;
        
        /// <summary>
        /// Register of instantiated objects.
        /// </summary>
        private ObjectsRegister register;
        
        public AssetReference Prefab => prefab;
        public int MaxInstancesAmount => maxInstancesAmount;
        public bool DestroyOverflowInstances => destroyOverflowInstances;
        public float OverflowInstancesLifeTime => overflowInstancesLifeTime;
        public bool InstantiateObjectOnAwake => instantiateObjectOnAwake;

        public ObjectsRegister Register
        {
            get
            {
                if (registerCreated == false)
                {
                    register = new ObjectsRegister();
                    registerCreated = true;
                }

                return register;
            }
        }

        public void RegisterObject(PoolObject _poolObject)
        {
            instancesCount++;
            Debug.Log(instancesCount);

            register.AddObjectToRegister(_poolObject);
        }

        public void UnregisterObject(PoolObject _poolObject)
        {
            instancesCount--;
            register.RemoveObjectFromRegister(_poolObject);
        }
        
        public bool CheckSpace()
        {
            if (instancesCount >= maxInstancesAmount)
                return false;

            return true;
        }
    }
}