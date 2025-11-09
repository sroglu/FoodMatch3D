using System;
using UnityEngine;

namespace Game.Data
{
    public enum GameAction
    {
        None,
        PlayPauseToggle,
        PuzzleObjectClick,
    }
    
    public struct GameActionData : ICloneable
    {
        public GameAction ActionType;
        public Vector3Int TargetPosition;
        
        public GameActionData(GameAction actionType, Vector3Int targetPosition)
        {
            ActionType = actionType;
            TargetPosition = targetPosition;
        }
        public GameActionData(GameAction actionType)
        {
            ActionType = actionType;
            TargetPosition = Vector3Int.zero;
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}