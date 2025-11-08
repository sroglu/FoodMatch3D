using UnityEngine;

namespace Game.Data
{
    public enum GameAction
    {
        None,
        TileClicked,
        ShuffleRequested,
        HintRequested,
        LevelRestartRequested
    }
    
    public record GameActionData
    {
        public GameAction ActionType;
        public Vector3Int TargetPosition;
    }
}