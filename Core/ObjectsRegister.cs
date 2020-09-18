using System.Collections.Generic;

namespace ObjectPool.Core
{
    public struct ObjectsRegister
    {
        private bool registerAllocated;
        private List<PoolObject> instantiatedObjects;

        /// <summary>
        /// Try get available instantiated object from pool.
        /// </summary>
        /// <param name="_poolObject"> Return pool object. </param>
        /// <returns> True if available pool object is found.
        /// False if there is no available objects in pool. </returns>
        public bool TryGetAvailableObject(ref PoolObject _poolObject)
        {
            ValidateRegister();
            
            for (int i = 0; i < instantiatedObjects.Count; i++)
            {
                if (instantiatedObjects[i].IsActivated == false)
                {
                    _poolObject = instantiatedObjects[i];
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Add pool object to list of instantiated objects.
        /// </summary>
        /// <param name="_poolObject"> Instantiated pool object. </param>
        public void AddObjectToRegister(PoolObject _poolObject)
        {
            ValidateRegister();
            
            instantiatedObjects.Add(_poolObject);
        }

        /// <summary>
        /// Remove object from list of instantiated objects.
        /// </summary>
        /// <param name="_poolObject"> Instantiated pool object.</param>
        public void RemoveObjectFromRegister(PoolObject _poolObject)
        {
            instantiatedObjects.Remove(_poolObject);
        }

        /// <summary>
        /// Check if list is allocated and allocate if not.
        /// </summary>
        private void ValidateRegister()
        {
            if (registerAllocated == false)
            {
                AllocateRegister();
                registerAllocated = true;
            }
        }
        
        /// <summary>
        /// Allocate pool objects list;
        /// </summary>
        private void AllocateRegister()
        {
            instantiatedObjects = new List<PoolObject>();
        }
    }
}