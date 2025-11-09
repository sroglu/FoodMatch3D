using UnityEngine;

namespace Game.Instances.PuzzleInstances
{
    public class PuzzleObjectInstance : PooledObjectBehaviour
    {
        public uint TypeId;
        public Vector3 InitialLocalPosition;
        public Quaternion InitialLocalRotation;
        public Vector3 InitialLocalScale = Vector3.one;
        
        public virtual void Initialize(uint typeId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            TypeId = typeId;
            InitialLocalPosition = position;
            InitialLocalRotation = rotation;
            InitialLocalScale = scale;
            
            transform.localPosition = position;
            transform.localRotation = rotation;
            transform.localScale = scale;
        }
    }
}