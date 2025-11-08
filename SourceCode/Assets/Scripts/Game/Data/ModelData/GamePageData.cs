using System;

namespace Game.Data
{
    public class GamePageData : ICloneable
    {
        public Level Level;

        public GamePageData(Level level)
        {
            Level = level;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}