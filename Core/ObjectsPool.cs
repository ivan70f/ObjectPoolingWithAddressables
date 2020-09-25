using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ObjectPool.Core
{
    [RequireComponent(typeof(PoolObjectsDestroyer))]
    public class ObjectsPool : MonoBehaviour
    {
        [SerializeField] private PoolObjectData[] objectPools = new PoolObjectData[]{};

        private PoolObjectsDestroyer destroyer;

        private delegate void ObjectInstatniationCallback(InvokeData _invokeData);

        private void Awake()
        {
            destroyer = GetComponent<PoolObjectsDestroyer>();
        }

        /// <summary>
        /// Initialize pools on game start.
        /// </summary>
        private void Start()
        {
            InitializePools();
        }

        /// <summary>
        /// Get object from pool or instantiate new.
        /// </summary>
        /// <param name="_prefab"> Prefab asset reference. </param>
        /// <param name="_callBack"></param>
        /// <returns> False if there is no this prefab type in PoolHandler setup.
        /// True if you setted this type up. </returns>
        public bool  TryGetObject(AssetReference _prefab, Action<PoolObject> _callBack )
        {
            return InitializeObject(_prefab,  _callBack);
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

        public void GetObject(AssetReference _prefab)
        {
            InitializeObject(_prefab, null);
        }

        /// <summary>
        /// Return object to pool and deactivate it.
        /// </summary>
        /// <param name="_poolObject"> Object to return. </param>
        private void ReturnToPool(PoolObject _poolObject)
        {
            MovePoolObjectToHandler(_poolObject);
            _poolObject.OnHandlerReturnInvoke -= ReturnToPool;

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
        private bool InitializeObject(AssetReference _prefab, Action<PoolObject> _callBack)
        {
            PoolObject _poolObject = null;
            int _poolIndex = -1;

            if (TryFindPoolObjectDataByAssetReference(_prefab, ref _poolIndex) == false)
            {
                Debug.LogWarning("There is no this prefab type in setup");
                return false;
            }

            ObjectsRegister _register = objectPools[_poolIndex].Register;

            if (_register.TryGetAvailableObject(ref _poolObject) == true)
            {
                objectPools[_poolIndex].UpdateRegister(_register);
                _poolObject.GetFromPool(_poolIndex);
                _poolObject.OnHandlerReturnInvoke += ReturnToPool;
                _callBack?.Invoke(_poolObject);
                return true;
            }

            ObjectInstatniationCallback _instatniationCallback = InitializeNewPoolObject;
            
            InvokeData _invokeData = new InvokeData(_poolObject, _poolIndex, _callBack);

            StartCoroutine(InstantiateObject(_prefab, _instatniationCallback, _invokeData));

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

                for (int j = 0; j < objectPools[i].MaxInstancesAmount; j++)
                {
                    ObjectInstatniationCallback _callback = InitializeNewPoolObjectOnAwake;
                    
                    InvokeData _invokeData = new InvokeData(null, i, null);
                    
                    StartCoroutine(InstantiateObject(objectPools[i].Prefab, _callback, _invokeData));
                }
            }
        }

        /// <summary>
        /// Find pool object data in array by asset reference assigned in PoolObjectData struct.
        /// </summary>
        /// <param name="_prefab"> Prefab asset reference. </param>
        /// <param name="poolIndex"> Index of used pool</param>
        /// <returns> False if there is no this prefab type in PoolHandler setup.
        /// True if you setted this type up.</returns>
        private bool TryFindPoolObjectDataByAssetReference(AssetReference _prefab, ref int poolIndex)
        {
            for (int i = 0; i < objectPools.Length; i++)
            {
                if (objectPools[i].Prefab.AssetGUID.Equals(_prefab.AssetGUID))
                {
                    poolIndex = i;
                    return true;
                }
                
            }
            Debug.Log("There is no pool for this prefab");

            return false;
        }

        /// <summary>
        /// Initialize new instantiated pool object.
        /// </summary>
        private void InitializeNewPoolObject(InvokeData _invokeData)
        {
            objectPools[_invokeData.PoolIndex].RegisterObject(_invokeData.PoolObject);
            _invokeData.PoolObject.GetFromPool(_invokeData.PoolIndex);
            _invokeData.PoolObject.OnHandlerReturnInvoke += ReturnToPool;
            _invokeData.Callback?.Invoke(_invokeData.PoolObject);
        }
        
        /// <summary>
        /// Initialize new instantiated pool object on Awake.
        /// </summary>
        private void InitializeNewPoolObjectOnAwake(InvokeData _invokeData)
        {
            objectPools[_invokeData.PoolIndex].RegisterObject(_invokeData.PoolObject);
            _invokeData.PoolObject.gameObject.SetActive(false);
        }

        /// <summary>
        /// Instantiate prefab and make it as a child.
        /// </summary>
        /// <param name="_prefab"> Prefab to instantiate. </param>
        /// <param name="_callback"> Instantiation callback. </param>
        /// <param name="_invokeData"> Invoke callback. </param>
        private IEnumerator InstantiateObject(
            AssetReference _prefab,
            ObjectInstatniationCallback _callback,
            InvokeData _invokeData)
        {
            AsyncOperationHandle<GameObject> _handle = _prefab.InstantiateAsync(transform);

            while (_handle.Status == AsyncOperationStatus.None)
                yield return null;
            
            GameObject _object = _handle.Result;

            PoolObject _poolObject = _object.GetComponent<PoolObject>();

            _invokeData.PoolObject = _poolObject;
            
            _callback?.Invoke(_invokeData);
        }
        

        /// <summary>
        /// Make pool object as a child of this object.
        /// </summary>
        /// <param name="_poolObject"> Pool object. </param>
        private void MovePoolObjectToHandler(PoolObject _poolObject)
        {
            _poolObject.transform.parent = transform;
        }
        
        private struct InvokeData
        {
            public PoolObject PoolObject;
            public readonly int PoolIndex;
            public readonly Action<PoolObject> Callback;

            public InvokeData(PoolObject _poolObject, int _poolIndex, Action<PoolObject> _poolCallback)
            {
                PoolObject = _poolObject;
                PoolIndex = _poolIndex;
                Callback = _poolCallback;
            }
        }
    }
}