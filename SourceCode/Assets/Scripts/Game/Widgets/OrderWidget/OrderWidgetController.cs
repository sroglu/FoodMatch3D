using mehmetsrl.MVC.core;

namespace Game.Widgets.OrderWidget
{
    public class OrderWidgetController : Controller<OrderWidgetView, OrderWidgetModel>
    {
        public OrderWidgetController(OrderWidgetModel model, OrderWidgetView view = null) : base(ControllerType.Instance, model, view)
        {
        }
    }
}