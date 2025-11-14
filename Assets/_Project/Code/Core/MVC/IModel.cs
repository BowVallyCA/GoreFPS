using System;

namespace _Project.Code.Core.MVC
{
    public interface IModel
    {
        event Action OnDataChanged;
        void Initialize();
        void Dispose();
    }
    
    public interface IModel<T> : IModel where T : class
    {
        T Data { get; }
        void SetData(T data);
    }
}