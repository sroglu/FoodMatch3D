using System;

namespace Game.Data
{
    public class GamePageData : ICloneable
    {
        public LevelData Level;
        public bool IsLevelLoaded => Level != null;

        public GamePageData(LevelData level)
        {
            Level = level;
        }
        public GamePageData() { }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}