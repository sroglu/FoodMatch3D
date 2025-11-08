using System;

namespace Game.Data
{
    public class GamePageData : ICloneable
    {
        public LevelData Level;

        public GamePageData(LevelData level)
        {
            Level = level;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}