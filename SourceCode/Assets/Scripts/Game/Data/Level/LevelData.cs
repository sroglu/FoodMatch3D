using System;
using UnityEngine;

namespace Game.Data
{

    [Serializable]
    public class PuzzleObjectSerializationData
    {
        public uint TypeId;
        public uint Quantity;
        public bool IsOrdered;
        public Vector3[] Positions;
        public Quaternion[] Rotations;
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(TypeId);
            hash.Add(Quantity);
            if (Positions != null)
            {
                foreach (var pos in Positions)
                {
                    hash.Add(pos);
                }
            }
            else
            {
                hash.Add(0);
            }

            if (Rotations != null)
            {
                foreach (var rot in Rotations)
                {
                    hash.Add(rot);
                }
            }
            else
            {
                hash.Add(0);
            }

            return hash.ToHashCode();
        }
    }
    
    [Serializable]
    public class CameraData
    {
        public int SecondsToSolve;
        public Vector3 Position;
        public Quaternion Rotation;
        public float OrthographicSize;
        public float FarClipPlane;
    }

    [Serializable]
    public class LevelData
    {
        public LevelId Id;
        public PuzzleObjectSerializationData[] PuzzleObjects;
        public CameraData CameraData;
        public uint TimeLimitInSeconds = 90; // Default time limit is 90 seconds
    }
}