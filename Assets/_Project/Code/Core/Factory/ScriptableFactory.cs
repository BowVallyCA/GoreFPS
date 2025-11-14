using UnityEngine;

namespace _Project.Code.Core.Factory
{
    public abstract class ScriptableFactory<T> : ScriptableObject, IFactory<T>
    {
        public abstract T Create();
    }

    public abstract class ScriptableFactory<TParam, TProduct> : ScriptableObject, IFactory<TParam, TProduct>
    {
        public abstract TProduct Create(TParam param);
    }
}