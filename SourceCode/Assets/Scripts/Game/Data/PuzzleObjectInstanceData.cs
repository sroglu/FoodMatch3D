using UnityEngine;

namespace Game.Data
{
    public class PuzzleObjectInstanceData
    {
        public uint TypeId;
        public Vector3 InitialPosition;
        public Quaternion InitialRotation;
        public Vector3 InitialScale = Vector3.one;
    }
}