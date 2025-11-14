using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Core.ObjectPool
{
    public class ObjectPool<T> where T : Component
    {
        private readonly Queue<T> _pool = new();
        private readonly Func<T> _createFunc;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onDestroy;
        private readonly int _maxSize;
        private readonly bool _collectionCheck;
        private readonly Transform _container;
        private int _activeCount;

        public int CountInactive => _pool.Count;
        public int CountActive => _activeCount;
        public int CountAll => CountInactive + CountActive;

        public ObjectPool(
            Func<T> createFunc,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            Action<T> onDestroy = null,
            bool collectionCheck = true,
            int defaultCapacity = 10,
            int maxSize = 10000)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _onGet = onGet;
            _onRelease = onRelease;
            _onDestroy = onDestroy;
            _collectionCheck = collectionCheck;
            _maxSize = maxSize;

            // Create container for pooled objects
            var containerGo = new GameObject($"Pool - {typeof(T).Name}");
            _container = containerGo.transform;

            // Pre-populate pool
            for (int i = 0; i < defaultCapacity; i++)
            {
                var obj = CreateNewObject();
                obj.gameObject.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        public T Get()
        {
            T obj;
            
            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else
            {
                obj = CreateNewObject();
            }

            if (_collectionCheck && obj == null)
            {
                throw new InvalidOperationException("Pooled object was destroyed while in pool!");
            }

            _activeCount++;
            obj.gameObject.SetActive(true);
            
            _onGet?.Invoke(obj);
            
            if (obj is IPoolable poolable)
            {
                poolable.OnSpawnFromPool();
            }

            return obj;
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            var obj = Get();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            return obj;
        }

        public void Release(T obj)
        {
            if (obj == null) return;

            if (_collectionCheck && _pool.Contains(obj))
            {
                Debug.LogError("Object is already in pool!");
                return;
            }

            _activeCount--;
            
            if (obj is IPoolable poolable)
            {
                poolable.OnReturnToPool();
            }
            
            _onRelease?.Invoke(obj);
            
            if (_pool.Count < _maxSize)
            {
                obj.transform.SetParent(_container);
                obj.gameObject.SetActive(false);
                _pool.Enqueue(obj);
            }
            else
            {
                _onDestroy?.Invoke(obj);
                UnityEngine.Object.Destroy(obj.gameObject);
            }
        }

        public void Clear()
        {
            foreach (var obj in _pool)
            {
                if (obj != null)
                {
                    _onDestroy?.Invoke(obj);
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
            }
            
            _pool.Clear();
            _activeCount = 0;
            
            if (_container != null)
            {
                UnityEngine.Object.Destroy(_container.gameObject);
            }
        }

        private T CreateNewObject()
        {
            var obj = _createFunc();
            if (obj == null)
            {
                throw new InvalidOperationException("Create function returned null!");
            }
            
            obj.transform.SetParent(_container);
            return obj;
        }
    }
}