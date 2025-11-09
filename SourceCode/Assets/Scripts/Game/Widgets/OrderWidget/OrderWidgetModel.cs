using Game.Data;
using mehmetsrl.MVC.core;

namespace Game.Widgets.OrderWidget
{
    
    public class OrderWidgetModel : Model<OrderData>
    {
        public OrderWidgetModel(OrderData[] dataArr) : base(dataArr)
        {
        }
    }
}