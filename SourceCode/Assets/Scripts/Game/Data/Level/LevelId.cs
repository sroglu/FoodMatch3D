using System;

namespace Game.Data
{
    public struct LevelId : IEquatable<LevelId>
    {
        public int Value;

        public LevelId(int value)
        {
            Value = value;
        }
        
        public static LevelId One = new LevelId(1);

        public override string ToString()
        {
            return $"level_{Value}";
        }

        public bool Equals(LevelId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is LevelId other && Equals(other);
        }
        
        public static bool operator == (LevelId left, LevelId right)
        {
            return left.Value == right.Value;
        }
        
        public static bool operator != (LevelId left, LevelId right)
        {
            return left.Value != right.Value;
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}