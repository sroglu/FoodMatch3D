using System;
using UnityEngine;

namespace Game.Data
{
    /*[Serializable]
    public class Level
    {
        public string Name;
        public LevelId Id;
        public PuzzleObject[] PuzzleObjects;
    }*/

    [Serializable]
    public class PuzzleObject
    {
        public uint TypeId;
        public uint Quantity;
        public Vector3[] Positions;
        public Quaternion[] Rotations;
        public override int GetHashCode()
        {
            return HashCode.Combine(TypeId, Quantity);
        }
    }
    
    [Serializable]
    public class CameraData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float OrthographicSize;
        public float FarClipPlane;
    }

    [Serializable]
    public class LevelData
    {
        public LevelId Id;
        public PuzzleObject[] PuzzleObjects;
        public CameraData CameraData;
    }
}