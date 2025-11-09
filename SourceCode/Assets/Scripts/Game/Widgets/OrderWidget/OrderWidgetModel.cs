using Game.Data;
using mehmetsrl.MVC.core;

namespace Game.Widgets.OrderWidget
{
    
    public class OrderWidgetModel : Model<OrderData>
    {
        public uint OrderCount { get; private set; }
        public OrderWidgetModel(OrderData[] dataArr) : base(dataArr)
        {
            OrderCount = (uint)dataArr.Length;
        }
        
        public void DecrementOrderCount()
        {
            if (OrderCount > 0)
            {
                OrderCount--;
            }
        }
    }
}