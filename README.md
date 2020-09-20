# ObjectPoolingWithAddressables
Unity object pooling system with addressables unity package.

Quick goide:
1. Install adressables package in package manager.

2. Create prefab. Attach to it component PoolObject. Mark prefab as adressable.

3. Add to object on your scene component ObjectsPool. Component PoolObjectsDestroyer wiil be added automatically.

4. Add to array your prefab with PoolObject component.

5. Get object from pool
```
[SerializeField] private AssetReference prefab;

private ObjectsPool pool;

private void Awake()
{
    pool = FindObjectOfType<ObjectsPool>();
}

...

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
    // Not that if there is actually available instantiated object in pool you will get callback immidietly.
    
    if (pool.TryGetObject(prefab, PoolCallback) == true)
        Debug.Log("Prefab spawned");
    else
        Debug.Log("Prefab wasn't spawned");
    
}

private GameObject spawnedObject;

private void PoolCallback(PoolObject _poolObject)
{
    spawnedObject = _poolObject.gameObject;
}

```

6. Return object to pool
```
spawnedObject.GetComponent<PoolObject>().ReturnToPool();
```

7. You can also use events of PoolObject.

```
private void SpawnObject() 
{
    if (pool.TryGetObject(prefab, PoolCallback) == true)
        Debug.Log("Prefab spawned");
    else
        Debug.Log("Prefab wasn't spawned");
    
}

private PoolObject spawnedObject;

private void PoolCallback(PoolObject _poolObject)
{
    spawnedObject = _poolObject.

    _poolObject.OnTakeFromPool += OnSpawn;
    _poolObject.OnReturnToPool += OnReturn;
}

private void OnSpawn() 
{
    ...
}

private void OnReturn()
{
    // Dont forget to unsubscribe events.
    poolObject.OnTakeFromPool -= OnSpawn;
    poolObject.OnReturnToPool -= OnReturn;
    ...
}
```
