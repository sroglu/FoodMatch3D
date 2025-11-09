using System;
using System.Collections.Generic;
using Game.Constants;
using Game.Data;
using mehmetsrl.DataStore;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.DataStores
{
    public class GameDataStore : DataStoreClass<GameDataStore>
    {
        public int CurrentLevelId => _playerData.CurrentLevelId.Value;
        public Data.GameData GameData { get; private set; }

        private PlayerData _playerData;

        public Queue<GameActionData> GameActionQueue { get; } = new();

        private IReadOnlyList<PuzzleObjectInstanceData> _puzzleObjects = Array.Empty<PuzzleObjectInstanceData>();
        public IReadOnlyList<PuzzleObjectInstanceData> PuzzleObjects => _puzzleObjects;

        protected override void OnInitialized()
        {
            _playerData = new PlayerData()
            {
                Name = PlayerPrefs.GetString(PlayerPrefsKeys.PlayerNameKey, "New Player"),
                CurrentLevelId = new LevelId(PlayerPrefs.GetInt(PlayerPrefsKeys.PlayerLevelKey, 0)),
                Coins = (uint)PlayerPrefs.GetInt(PlayerPrefsKeys.PlayerCoinsKey, 0),
                SoundOn = PlayerPrefs.GetInt(PlayerPrefsKeys.SoundVolumeKey, 1) == 1,
                MusicOn = PlayerPrefs.GetInt(PlayerPrefsKeys.MusicVolumeKey, 1) == 1,
                IsTutorialCompleted = PlayerPrefs.GetInt(PlayerPrefsKeys.IsTutorialCompletedKey, 0) == 1
            };
        }

        public void SetGameData(Data.GameData gameData)
        {
            GameData = gameData;
        }

        public void UpdatePlayerDataOnLevelComplete()
        {
            _playerData.CurrentLevelId.Value += 1;
            _playerData.Coins += GameData.LevelCompleteRewardCoins;

            PlayerPrefs.SetInt(PlayerPrefsKeys.PlayerLevelKey, _playerData.CurrentLevelId.Value);
            PlayerPrefs.SetInt(PlayerPrefsKeys.PlayerCoinsKey, (int)_playerData.Coins);
            PlayerPrefs.Save();
        }

        public void SetSlotState(IEnumerable<PuzzleObjectInstanceData> items)
        {
            if (items == null)
            {
                _puzzleObjects = Array.Empty<PuzzleObjectInstanceData>();
                return;
            }

            var copy = new List<PuzzleObjectInstanceData>(items);
            if (copy.Count == 0)
            {
                _puzzleObjects = Array.Empty<PuzzleObjectInstanceData>();
                return;
            }

            _puzzleObjects = copy.AsReadOnly();
        }


#if UNITY_EDITOR

        [MenuItem("Tool/Game/Clear PlayerPrefs")]
        public static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("PlayerPrefs cleared.");
        }
#endif
    }
}