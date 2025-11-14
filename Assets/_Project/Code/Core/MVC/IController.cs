namespace _Project.Code.Core.MVC
{
    public interface IController
    {
        void Initialize();
        void Enable();
        void Disable();
        void Dispose();
    }
    
    public interface IController<TModel, TView> : IController 
        where TModel : IModel 
        where TView : IView
    {
        TModel Model { get; }
        TView View { get; }
    }
}