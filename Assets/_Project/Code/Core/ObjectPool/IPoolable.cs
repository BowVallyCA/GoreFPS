namespace _Project.Code.Core.ObjectPool
{
    public interface IPoolable
    {
        void OnSpawnFromPool();
        void OnReturnToPool();
    }
}