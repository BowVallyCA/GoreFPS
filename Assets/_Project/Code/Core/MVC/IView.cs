namespace _Project.Code.Core.MVC
{
    public interface IView
    {
        void Initialize();
        void Show();
        void Hide();
        void UpdateDisplay();
        void Dispose();
    }
    
    public interface IView<T> : IView where T : class
    {
        void UpdateDisplay(T data);
    }
}