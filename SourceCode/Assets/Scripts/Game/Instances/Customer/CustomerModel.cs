using Game.Data;
using mehmetsrl.MVC.core;
public class CustomerModel : Model<CustomerViewData>
{
    public uint OrderId { get; private set; }
    public uint Quantity { get; private set; }
    public CustomerModel(CustomerViewData data) : base(data)
    {
    }
    public void SetOrder(uint orderId, uint quantity)
    {
        OrderId = orderId;
        Quantity = quantity;
    }
}
