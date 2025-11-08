using System.Collections.Generic;
using Game.Data;
using mehmetsrl.DataStore;
using UnityEngine;

namespace Game.DataStores
{
    public class GameDataStore : DataStoreClass<GameDataStore>
    {
        public int CurrentLevelID { get; set; }
        public LevelData LevelData { get; private set; }
        public Data.GameData GameData { get; private set; }
        
        public Queue<GameActionData> GameActionQueue { get; } = new();
        
        public void SetLevelData(LevelData levelData)
        {
            LevelData = levelData;
        }
        public void SetGameData(Data.GameData gameData)
        {
            GameData = gameData;
        }

        public Level GetLevelDataById(LevelId levelId)
        {
            Debug.Assert(LevelData != null, "LevelData is not set in GameDataStore.");
            if (!LevelData.TryGetLevelById(levelId, out var level))
            {
                Debug.LogError($"Level with Id {levelId.Value} not found.");
                return null;
            }
            return level;
        }
    }
}