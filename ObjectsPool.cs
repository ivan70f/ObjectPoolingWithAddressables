using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ObjectPool
{
    [RequireComponent(typeof(PoolObjectsDestroyer))]
    public class ObjectsPool : MonoBehaviour
    {
        [SerializeField] private PoolObjectData[] objectPools = new PoolObjectData[0];

        private PoolObjectsDestroyer destroyer;

        private delegate void ObjectInstatniationCallback(PoolObject _poolObject, int _poolIndex, Action<bool, PoolObject> _poolCallback);

        private void Awake()
        {
            destroyer = GetComponent<PoolObjectsDestroyer>();
            
            Assert.IsNull(destroyer);
        }

        /// <summary>
        /// Initialize pools on game start.
        /// </summary>
        private void Start()
        {
            for (int i = 0; i < objectPools.Length; i++)
            {
                AsyncOperationHandle _handle = objectPools[0].Prefab.LoadAssetAsync<GameObject>();
            }
 
            InitializePools();
        }

        /// <summary>
        /// Get object from pool or instantiate new.
        /// </summary>
        /// <param name="_prefab"> Prefab asset reference. </param>
        /// <param name="_callBack"></param>
        /// <returns> False if there is no this prefab type in PoolHandler setup.
        /// True if you setted this type up. </returns>
        public void  TryGetObject(AssetReference _prefab, Action<bool, PoolObject> _callBack )
        {
            InitializeObject(_prefab,  _callBack);
        }
        
        /// <summary>
        /// Get object from pool or instantiate new.
        /// </summary>
        /// <param name="_prefab"> Prefab asset reference. </param>
        /// <returns> False if there is no this prefab type in PoolHandler setup.
        /// True if you setted this type up. </returns>
        public bool TryGetObject(AssetReference _prefab)
        {
            return InitializeObject(_prefab, null);
        }

        /// <summary>
        /// Return object to pool and deactivate it.
        /// </summary>
        /// <param name="_poolObject"> Object to return. </param>
        private void ReturnToPool(PoolObject _poolObject)
        {
            MovePoolObjectToHandler(_poolObject);
            _poolObject.OnHandlerReturnInvoke -= ReturnToPool;

            Debug.Log(objectPools[_poolObject.PoolIndex].CheckSpace());

            if (objectPools[_poolObject.PoolIndex].CheckSpace() == false && 
                objectPools[_poolObject.PoolIndex].DestroyOverflowInstances == true)
            {
                objectPools[_poolObject.PoolIndex].UnregisterObject(_poolObject);

                float _destroyDelay = objectPools[_poolObject.PoolIndex].OverflowInstancesLifeTime;
                destroyer.DestroyPoolObjectOnDelay(_poolObject,_destroyDelay);
            }
        }

        /// <summary>
        /// Initialize selected object.
        /// </summary>
        /// <param name="_prefab"> Prefab asset reference. </param>
        /// <param name="_callBack"></param>
        /// <returns> False if there is no this prefab type in PoolHandler setup.
        /// True if you setted this type up. </returns>
        private bool InitializeObject(AssetReference _prefab, Action<bool, PoolObject> _callBack)
        {
            PoolObjectData _poolObjectData = new PoolObjectData();

            PoolObject _poolObject = null;
            int _poolIndex = -1;

            if (TryFindPoolObjectDataByAssetReference(_prefab, ref _poolObjectData, ref _poolIndex) == false)
            {
                Debug.LogWarning("There is no this prefab type in setup");
                return false;
            }

            if (_poolObjectData.Register.TryGetAvailableObject(ref _poolObject) == true)
                return true;
            Debug.Log(_poolObjectData.MaxInstancesAmount);
            ObjectInstatniationCallback _instatniationCallback = InitializeNewPoolObject;
            StartCoroutine(InstantiateObject(_prefab, _poolIndex, _instatniationCallback,_callBack));

            return true;
        }
        
        /// <summary>
        /// Initialize all pre-instantiated objects.
        /// </summary>
        private void InitializePools()
        {
            for (int i = 0; i < objectPools.Length; i++)
            {
                if (objectPools[i].InstantiateObjectOnAwake == false)
                    continue;

                for (int j = 0; j < objectPools[j].MaxInstancesAmount; j++)
                {
                    ObjectInstatniationCallback _callback = InitializeNewPoolObject;
                    StartCoroutine(InstantiateObject(objectPools[j].Prefab, i, _callback, null));
                }
            }
        }

        /// <summary>
        /// Find pool object data in array by asset reference assigned in PoolObjectData struct.
        /// </summary>
        /// <param name="_prefab"> Prefab asset reference. </param>
        /// <param name="poolObjectData"> Return pool object data. </param>
        /// <param name="poolIndex"> Index of used pool</param>
        /// <returns> False if there is no this prefab type in PoolHandler setup.
        /// True if you setted this type up.</returns>
        private bool TryFindPoolObjectDataByAssetReference(AssetReference _prefab, ref PoolObjectData poolObjectData,
            ref int poolIndex)
        {
            for (int i = 0; i < objectPools.Length; i++)
            {
                if (objectPools[i].Prefab.AssetGUID.Equals(_prefab.AssetGUID))
                {
                    poolObjectData = objectPools[i];
                    poolIndex = i;
                    return true;
                }
                
            }
            Debug.Log("There is no pool for this prefab");

            return false;
        }

        private void InitializeNewPoolObject(PoolObject _poolObject, int _poolIndex, Action<bool, PoolObject> _poolCallback)
        {
            Debug.Log(0);
            objectPools[_poolIndex].RegisterObject(_poolObject);
            _poolObject.GetFromPool(_poolIndex);
            _poolObject.OnHandlerReturnInvoke += ReturnToPool;
            _poolCallback?.Invoke(true, _poolObject);
        }

        /// <summary>
        /// Instantiate prefab and make it as a child.
        /// </summary>
        /// <param name="_prefab"> Prefab to instantiate. </param>
        /// <param name="_poolIndex"></param>
        /// <param name="_callback"></param>
        /// <param name="_poolCallback"></param>
        private IEnumerator InstantiateObject(
            AssetReference _prefab, 
            int _poolIndex,
            ObjectInstatniationCallback _callback,
            Action<bool, PoolObject> _poolCallback)
        {
            AsyncOperationHandle<GameObject> _handle = _prefab.InstantiateAsync(transform);

            while (_handle.Status == AsyncOperationStatus.None)
                yield return null;
            
            GameObject _object = _handle.Result;

            PoolObject _poolObject = _object.GetComponent<PoolObject>();
            
            _callback?.Invoke(_poolObject, _poolIndex,_poolCallback);
        }
        

        private void MovePoolObjectToHandler(PoolObject _poolObject)
        {
            _poolObject.transform.parent = transform;
        }
    }
}