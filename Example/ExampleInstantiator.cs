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
            yield return new WaitForSeconds(20);
            
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
    }
}