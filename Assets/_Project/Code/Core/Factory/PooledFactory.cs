using _Project.Code.Core.ObjectPool;
using UnityEngine;

namespace _Project.Code.Core.Factory
{
    public class PooledFactory<T> : IFactory<T> where T : Component
    {
        private readonly ObjectPool<T> _pool;

        public PooledFactory(T prefab, int initialCapacity = 10, int maxSize = 100)
        {
            _pool = new ObjectPool<T>(
                createFunc: () => Object.Instantiate(prefab),
                defaultCapacity: initialCapacity,
                maxSize: maxSize
            );
        }

        public T Create()
        {
            return _pool.Get();
        }

        public T Create(Vector3 position, Quaternion rotation)
        {
            return _pool.Get(position, rotation);
        }

        public void Return(T obj)
        {
            _pool.Release(obj);
        }

        public void Clear()
        {
            _pool.Clear();
        }
    }
}