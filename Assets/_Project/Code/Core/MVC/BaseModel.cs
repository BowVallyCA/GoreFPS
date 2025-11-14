using System;

namespace _Project.Code.Core.MVC
{
    public abstract class BaseModel<T> : IModel<T> where T : class
    {
        public event Action OnDataChanged;
        
        protected T _data;
        
        public T Data => _data;
        
        public virtual void Initialize()
        {
        }
        
        public virtual void SetData(T data)
        {
            _data = data;
            OnDataChanged?.Invoke();
        }
        
        protected void NotifyDataChanged()
        {
            OnDataChanged?.Invoke();
        }
        
        public virtual void Dispose()
        {
            OnDataChanged = null;
        }
    }
}