using System.Collections.Generic;
using Game.Data;
using mehmetsrl.DataStore;
using UnityEngine;

namespace Game.DataStores
{
    public class GameDataStore : DataStoreClass<GameDataStore>
    {
        public int CurrentLevelID { get; set; }
        public Data.GameData GameData { get; private set; }
        
        public Queue<GameActionData> GameActionQueue { get; } = new();
        
        public void SetGameData(Data.GameData gameData)
        {
            GameData = gameData;
        }
    }
}