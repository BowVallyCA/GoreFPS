namespace _Project.Code.Core.Factory
{
    public interface IFactory<out T>
    {
        T Create();
    }

    public interface IFactory<in TParam, out TProduct>
    {
        TProduct Create(TParam param);
    }
}