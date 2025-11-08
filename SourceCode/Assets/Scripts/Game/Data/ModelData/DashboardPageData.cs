using System;

namespace Game.Data.ModelData
{
    public class DashboardPageData : ICloneable
    {
        public PlayerData PlayerData;
        
        public DashboardPageData(PlayerData playerData)
        {
            PlayerData = playerData;
        }
        
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}