using UnityEngine;

namespace _Project.Code.Core.ObjectPool
{
    public abstract class PoolableMonoBehaviour : MonoBehaviour, IPoolable
    {
        public virtual void OnSpawnFromPool()
        {
            // Override in derived classes
        }

        public virtual void OnReturnToPool()
        {
            // Override in derived classes
        }
    }
}