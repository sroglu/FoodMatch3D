using System;

namespace Game.Data.ModelData
{
    public class DashboardPageData : ICloneable
    {
        public DashboardPageData() { }
        
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}