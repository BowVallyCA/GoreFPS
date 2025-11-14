namespace _Project.Code.Core.MVC
{
    public abstract class BaseController<TModel, TView> : IController<TModel, TView> 
        where TModel : IModel 
        where TView : IView
    {
        public TModel Model { get; protected set; }
        public TView View { get; protected set; }
        
        private bool _isEnabled;
        
        public BaseController(TModel model, TView view)
        {
            Model = model;
            View = view;
        }
        
        public virtual void Initialize()
        {
            Model?.Initialize();
            View?.Initialize();
            
            if (Model != null)
            {
                Model.OnDataChanged += OnModelDataChanged;
            }
        }
        
        public virtual void Enable()
        {
            if (_isEnabled) return;
            
            _isEnabled = true;
            View?.Show();
            OnEnabled();
        }
        
        public virtual void Disable()
        {
            if (!_isEnabled) return;
            
            _isEnabled = false;
            View?.Hide();
            OnDisabled();
        }
        
        protected virtual void OnEnabled()
        {
            // Override in derived classes
        }
        
        protected virtual void OnDisabled()
        {
            // Override in derived classes
        }
        
        protected virtual void OnModelDataChanged()
        {
            if (_isEnabled)
            {
                View?.UpdateDisplay();
            }
        }
        
        public virtual void Dispose()
        {
            if (Model != null)
            {
                Model.OnDataChanged -= OnModelDataChanged;
                Model.Dispose();
            }
            
            View?.Dispose();
        }
    }
}