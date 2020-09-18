using System.Collections;
using System.Collections.Generic;
using ObjectPool.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ObjectPool.Example
{
    public class ExampleInstantiator : MonoBehaviour
    {
        [SerializeField] private AssetReference prefab;
        
        private ObjectsPool pool;

        private List<PoolObject> objects;

        private void Awake()
        {
            pool = FindObjectOfType<ObjectsPool>();
            objects = new List<PoolObject>();
        }

        private void Start()
        {
            for (int i = 0; i < 15; i++)
            {
                pool.TryGetObject(prefab, HandlePoolCallback);
            }

            StartCoroutine(DestroyObjects());
            StartCoroutine(SpawnMore());

        }

        private IEnumerator DestroyObjects()
        {
            yield return new WaitForSeconds(3);
            
            for (int i = 0; i < objects.Count; i++)
                objects[i].ReturnToPool();
        }

        private IEnumerator SpawnMore()
        {
            yield return new WaitForSeconds(1);
            for (int i = 0; i < 8; i++)
            {
                pool.TryGetObject(prefab, HandlePoolCallback);
            }
        }

        private void HandlePoolCallback(PoolObject _poolObject)
        {
            objects.Add(_poolObject);
        }
        private void SpawnObject()
        {
            // Spawn object without any callback.
            // You can get an error if you forgot to add this prefab to ObjectPool component. 
            // In this way you cant catch and process this error.
    
            pool.GetObject(prefab);
    
            // Spawn object and check if it was spawned. 
            // Returns true if object was spawned.
    
            if (pool.TryGetObject(prefab) == true)
                Debug.Log("Prefab spawned");
            else
                Debug.Log("Prefab wasn't spawned");
        
            // Spawn object and get a callback with this object.
            // Due to adressables asynchronous loading you can't get object in moment you get it from pool,
            // because unity need a bit of time to load it.
    
            if (pool.TryGetObject(prefab, PoolCallback) == true)
                Debug.Log("Prefab spawned");
            else
                Debug.Log("Prefab wasn't spawned");
    
        }
        
        private GameObject spawnedObject;

        private void PoolCallback(PoolObject _poolObject)
        {
            spawnedObject = _poolObject.gameObject;
            
            
            _poolObject.ReturnToPool();
            
            
        }
    }
}