using System;

namespace Game.Data
{
    public class OrderData : ICloneable
    {
        public uint CustomerId;
        public uint OrderId;
        public uint Quantity;
        public bool IsCompleted;
        
        public OrderData(uint customerId, uint orderId, uint quantity)
        {
            CustomerId = customerId;
            OrderId = orderId;
            Quantity = quantity;
            IsCompleted = false;
        }
        
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}