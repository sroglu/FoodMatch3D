using System;

namespace Game.Data
{
    public class OrderData : ICloneable
    {
        public uint CustomerId;
        public uint OrderId;
        public uint Quantity;
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}