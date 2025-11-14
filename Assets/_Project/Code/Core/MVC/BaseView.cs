using UnityEngine;

namespace _Project.Code.Core.MVC
{
    public abstract class BaseView<T> : MonoBehaviour, IView<T> where T : class
    {
        [SerializeField] protected bool _startVisible = true;
        
        public virtual void Initialize()
        {
            if (_startVisible)
                Show();
            else
                Hide();
        }
        
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }
        
        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public virtual void UpdateDisplay()
        {
            // Override in derived classes
        }
        
        public abstract void UpdateDisplay(T data);
        
        public virtual void Dispose()
        {
            // Cleanup logic if needed
        }
    }
}