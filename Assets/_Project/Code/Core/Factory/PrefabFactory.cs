using UnityEngine;

namespace _Project.Code.Core.Factory
{
    public class PrefabFactory<T> : IFactory<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;

        public PrefabFactory(T prefab, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
        }

        public T Create()
        {
            return Object.Instantiate(_prefab, _parent);
        }

        public T Create(Vector3 position, Quaternion rotation)
        {
            return Object.Instantiate(_prefab, position, rotation, _parent);
        }
    }
}