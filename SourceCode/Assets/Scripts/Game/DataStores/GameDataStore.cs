using System;
using System.Collections.Generic;
using Game.Constants;
using Game.Data;
using Game.Instances.PuzzleInstances;
using mehmetsrl.DataStore;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.DataStores
{
    public class GameDataStore : DataStoreClass<GameDataStore>
    {
        public Camera GameCamera { get; private set; }
        public int CurrentLevelId => _playerData.CurrentLevelId.Value;
        public GameData GameData { get; private set; }

        public Action OnPuzzleObjectMatched;
        
        private PlayerData _playerData;
        private Queue<GameActionData> gameActionQueue { get; } = new();
        private Queue<uint> matchActionQueue { get; } = new();

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

        public void SetGameCamera(Camera camera)
        {
            GameCamera = camera;
        }
        public void SetGameData(Data.GameData gameData)
        {
            GameData = gameData;
        }

        #region Game Actions

        
        public void AddGameAction(GameActionData actionData)
        {
            gameActionQueue.Enqueue(actionData);
        }

        public bool TryConsumeGameAction(out GameActionData actionData)
        {
            if (gameActionQueue.Count == 0)
            {
                actionData = default;
                return false;
            }

            actionData = gameActionQueue.Dequeue();
            return true;
        }

        #endregion

        #region Match Actions

        
        public void AddClearMatchedPuzzleObject(uint typeId)
        {
            matchActionQueue.Enqueue(typeId);
            OnPuzzleObjectMatched?.Invoke();
        }

        public bool TryConsumeMatchAction(out uint typeId)
        {
            if (matchActionQueue.Count == 0)
            {
                typeId = uint.MaxValue;
                return false;
            }

            typeId = matchActionQueue.Dequeue();
            return true;
        }

        #endregion

        public void UpdatePlayerDataOnLevelComplete()
        {
            _playerData.CurrentLevelId.Value += 1;
            _playerData.Coins += GameData.LevelCompleteRewardCoins;

            PlayerPrefs.SetInt(PlayerPrefsKeys.PlayerLevelKey, _playerData.CurrentLevelId.Value);
            PlayerPrefs.SetInt(PlayerPrefsKeys.PlayerCoinsKey, (int)_playerData.Coins);
            PlayerPrefs.Save();
        }

        public bool TryUseCoinsOnKeepPlaying()
        {
            if (_playerData.Coins < GameData.KeepPlayingCostCoins)
            {
                return false;
            }
            
            _playerData.Coins -= GameData.KeepPlayingCostCoins;
            PlayerPrefs.SetInt(PlayerPrefsKeys.PlayerCoinsKey, (int)_playerData.Coins);
            PlayerPrefs.Save();
            return true;
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