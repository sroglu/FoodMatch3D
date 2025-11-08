using Game.Data;
using UnityEngine;

namespace Game
{
    public static class LevelUtils
    {
        public static LevelData LoadLevel(LevelId levelId, bool isEditor = false)
        {
            var levelName = levelId.ToString();
            var filePath =
                $"{Constants.LevelPaths.STREAMING_ASSETS_PATH}/{Constants.LevelPaths.LEVEL_FOLDER}/{levelName}.json";
            //if (!isEditor)
            {
                var isLastLevel = levelId.Value > GameData.LastLevelId;
                if (isLastLevel)
                {
                    var allLevelNames = System.IO.Directory.GetFiles(Constants.LevelPaths.STREAMING_ASSETS_PATH + $"/{Constants.LevelPaths.LEVEL_FOLDER}/", "level_*.json");
                    
                    var loopLevel = levelId.Value % GameData.LastLevelId - 1;
                    levelName = System.IO.Path.GetFileNameWithoutExtension(allLevelNames[loopLevel % allLevelNames.Length]);
                    filePath = $"{Constants.LevelPaths.STREAMING_ASSETS_PATH}/{Constants.LevelPaths.LEVEL_FOLDER}/{levelName}.json";
                }
            }

            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogError($"Level file not found: {filePath}");
                return null;
            }

            var json = System.IO.File.ReadAllText(filePath);
            var levelData = JsonUtility.FromJson<LevelData>(json);

            return levelData;
        }
    }
}