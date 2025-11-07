using System;

namespace mehmetsrl.MVC.core
{
    /// <summary>
    /// Controllers can be a page or an instance.
    /// </summary>
    public enum ControllerType
    {
        Page,
        Instance
    }

    /// <summary>
    /// Interface for controllers.
    /// A controller should have a model and a view.
    /// </summary>
    public interface IController : IDisposable
    {
        IModel GetModel();
        ViewBase GetView();
        void ShowView();
    }

    /// <summary>
    /// Base controller class with some common implementations
    /// It also describes functionalities of a controller
    /// </summary>
    public abstract class ControllerBase : IController
    {
        private static Action<IController, string, string, EventArgs> _redirectToAction;

        #region Properties
        protected readonly ControllerType ControllerType;

        #endregion
        protected ControllerBase(ControllerType controllerType)
        {
            ControllerType = controllerType;
            _redirectToAction += OnRedirectToAction;
        }

        #region UtilityFunctions    
        /// <summary>
        /// Getter function for model.
        /// </summary>
        /// <returns>Model</returns>
        public abstract IModel GetModel();
        /// <summary>
        /// Getter function for view.
        /// </summary>
        /// <returns>View</returns>
        public abstract ViewBase GetView();
        /// <summary>
        /// Facade function for view
        /// </summary>
        public abstract void ShowView();
        /// <summary>
        /// Redirect an event to all controllers
        /// If controller has implementation process it
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Additional data</param>
        protected void Redirect(string actionName, EventArgs data = null)
        {
            _redirectToAction(this, actionName, null, data);
        }

        /// <summary>
        /// Redirect an event to specified controller
        /// Controller should not be an instance type controller
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Target controller that handle the event</param>
        /// <param name="data">Additional data</param>
        protected void Redirect(string actionName, string controllerName, EventArgs data = null)
        {
            _redirectToAction(this, actionName, controllerName, data);
        }

        /// <summary>
        /// Distributes redirect calls.
        /// If controller name is null it triggers for all controllers
        /// </summary>
        /// <param name="source">Source controller that redirects event</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Target controller that handle the event</param>
        /// <param name="data">Additional data</param>
        void OnRedirectToAction(IController source, string actionName, string controllerName, EventArgs data)
        {
            if (controllerName == null || controllerName == GetType().ToString())
            {
                OnActionRedirected(source, actionName, data);
            }
        }
        #endregion

        #region Overridables
        /// <summary>
        /// Handle function for redirected events
        /// All controllers implement the events they responsible.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="actionName"></param>
        /// <param name="data"></param>
        protected virtual void OnActionRedirected(IController source, string actionName, EventArgs data) { }
        public abstract void Dispose();
        #endregion
    }

    /// <summary>
    /// Generic controller class
    /// It implements the relation with view and model
    /// </summary>
    /// <typeparam name="V"> View </typeparam>
    /// <typeparam name="M"> Model </typeparam>
    public abstract class Controller<V, M> : ControllerBase where V : ViewBase where M : IModel
    {
        private V _instanceView;
        private V _view;
        
        #region Accesors

        private V PageView => ViewManager.GetPageView<V>();
        private V InstanceView
        {
            get
            {
                if (_instanceView == null)
                    _instanceView = ViewManager.Instance.CreateInstanceView<V>();
                return _instanceView;
            }
        }

        public V View
        {
            get
            {
                if (_view == null)
                {
                    switch (ControllerType)
                    {
                        case ControllerType.Page:
                            _view = PageView;
                            break;
                        case ControllerType.Instance:
                            _view = InstanceView;
                            break;
                        default:
                            _view = PageView;
                            break;
                    }
                }
                return _view;
            }
            private set => _view = value;
        }
        protected M Model { get; private set; }
        
        #endregion

        protected Controller(ControllerType controllerType, M model, V view = null) : base(controllerType)
        {
            Model = model;
            View = view;

            View.Init(this);
            OnCreate();

            if (ControllerType == ControllerType.Page)
                View.Hide();
        }

        public sealed override void Dispose()
        {
            OnDestroy();
            Model.Dispose();
            View.Dispose();
        }
        public sealed override IModel GetModel()
        {
            return Model;
        }
        public sealed override ViewBase GetView()
        {
            return View;
        }
        public sealed override void ShowView()
        {
            View.Show();
        }

        #region Overridables
        protected virtual void OnCreate() { }
        protected virtual void OnDestroy() { }
        #endregion

    }
}