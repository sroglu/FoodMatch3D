using mehmetsrl.MVC.core;

namespace Game.Widgets.OrderWidget
{
    public class OrderWidgetController : Controller<OrderWidgetView, OrderWidgetModel>
    {
        public OrderWidgetController(ControllerType controllerType, OrderWidgetModel model, OrderWidgetView view = null) : base(controllerType, model, view)
        {
        }
    }
}