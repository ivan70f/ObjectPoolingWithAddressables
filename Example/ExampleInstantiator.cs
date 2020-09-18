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

        [SerializeField] private List<PoolObject> objects;

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
            
            for (int i = 0; i < 15; i++)
                objects[i].ReturnToPool();
        }

        private IEnumerator SpawnMore()
        {
            yield return new WaitForSeconds(10);
            for (int i = 0; i < 8; i++)
            {
                pool.TryGetObject(prefab, HandlePoolCallback);
            }
        }

        private void HandlePoolCallback(bool _instantiated, PoolObject _poolObject)
        {
            if (_instantiated == true)
                objects.Add(_poolObject);
        }
    }
}