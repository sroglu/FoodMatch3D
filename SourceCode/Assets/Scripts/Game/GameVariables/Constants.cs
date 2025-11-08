using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Constants for the game
/// </summary>
namespace Game.Constants
{
    public static class PlayerPrefsKeys
    {
        public static readonly string PlayerNameKey  =  "PlayerName";
        public static readonly string PlayerLevelKey =  "PlayerLevel";
        public static readonly string PlayerCoinsKey =  "PlayerCoins";
        
        public static readonly string SoundVolumeKey =  "SoundVolume";
        public static readonly string MusicVolumeKey =  "MusicVolume";
        public static readonly string IsTutorialCompletedKey =  "IsTutorialCompleted";
    }
    
    public static class LevelPaths
    {
        public static readonly string STREAMING_ASSETS_PATH = Application.streamingAssetsPath;
        public const string LEVEL_FOLDER = "Levels";
    }

    public static class Values
    {
        public static readonly Vector2Int InvalidVector2Int = Vector2Int.one * -1;
    }

    public static class Events
    {
        public static readonly List<string> All = new List<string> { 
            ClickOnTile, 
        };
        public const string ClickOnTile = "tile.click";
    }
    public static class Operations
    {
        public static readonly List<string> All = new List<string> {
            SpawnAgent, 
            Move,
            DealDamage,
        };
        public const string SpawnAgent = "operation.spawn.agent";
        public const string Move = "operation.move";
        public const string DealDamage = "operation.damage";
    }
    public static class Effects
    {
        public static readonly List<string> All = new List<string> {
            BoostAttackDamage,
            BoostAttackSpeed,
            BoostMovementSpeed,
            EngageEnemy,
            DealDamage,
            ProtectiveTower,
            ProtectiveWall,
        };
        public const string BoostAttackDamage = "effect.boost.attack.damage";
        public const string BoostAttackSpeed = "effect.boost.attack.speed";
        public const string BoostMovementSpeed = "effect.boost.movement.speed";
        public const string EngageEnemy = "effect.engage.enemy";
        public const string DealDamage = "effect.damage";
        public const string ProtectiveTower = "effect.tower";
        public const string ProtectiveWall = "effect.wall";
    }
    public static class Controllers
    {
        public static readonly List<string> All = new List<string> {
            GameBoardController,
            InfoController,
            ProductionController,
        };
        public const string GameBoardController = "GameBoardController";
        public const string InfoController = "InfoController";
        public const string ProductionController = "ProductionController";
    }
}