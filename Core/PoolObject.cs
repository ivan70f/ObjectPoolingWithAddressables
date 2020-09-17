using System;
using UnityEngine;

namespace ObjectPool
{
    public class PoolObject : MonoBehaviour
    {
        private bool isActivated;
        private int poolIndex;

        public bool IsActivated => isActivated;
        public int PoolIndex => poolIndex;
        
        public event Action OnTakeFromPool;
        public event Action OnReturnToPool;

        public event Action<PoolObject> OnHandlerReturnInvoke;
        
        public void ReturnToPool()
        {
            isActivated = false;
            
            gameObject.SetActive(false);
            
            OnReturnToPool?.Invoke();
            OnHandlerReturnInvoke?.Invoke(this);
        }
        
        public void GetFromPool(int _poolIndex)
        {
            isActivated = true;
            poolIndex = _poolIndex;
            
            gameObject.SetActive(true);
            
            OnTakeFromPool?.Invoke();
        }
    }
}