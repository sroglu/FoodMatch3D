using System;

namespace Game.Data
{
    public class OrderData : ICloneable
    {
        public int CustomerId;
        public int OrderId;
        public int Quantity;
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}