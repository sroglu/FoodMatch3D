using System;
using System.Collections;
using Game.Data;
using UnityEngine;

namespace Game
{
    public static class LevelUtils
    {
        #if UNITY_EDITOR
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
                    
                    var loopLevel = levelId.Value % GameData.LastLevelId;
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
        #endif


        public static IEnumerator LoadLevelAsync(LevelId levelId, Action<LevelData> onLoaded, bool isEditor = false)
        {
            var levelName = levelId.ToString();
            var filePath = 
                $"{Constants.LevelPaths.STREAMING_ASSETS_PATH}/{Constants.LevelPaths.LEVEL_FOLDER}/{levelName}.json";
            if (!isEditor)
            {
                var isLastLevel = levelId.Value > GameData.LastLevelId;
                if (isLastLevel)
                {
                    var allLevelNames = System.IO.Directory.GetFiles(Constants.LevelPaths.STREAMING_ASSETS_PATH + $"/{Constants.LevelPaths.LEVEL_FOLDER}/", "level_*.json");
                    var loopLevel = levelId.Value % GameData.LastLevelId;
                    levelName = System.IO.Path.GetFileNameWithoutExtension(allLevelNames[loopLevel % allLevelNames.Length]);
                    filePath = $"{Constants.LevelPaths.STREAMING_ASSETS_PATH}/{Constants.LevelPaths.LEVEL_FOLDER}/{levelName}.json";
                }
            }

            LevelData levelData;
#if UNITY_ANDROID && !UNITY_EDITOR
            var www = UnityEngine.Networking.UnityWebRequest.Get(filePath);
            yield return www.SendWebRequest();
            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                levelData = JsonUtility.FromJson<LevelData>(www.downloadHandler.text);
                onLoaded?.Invoke(levelData);
            }
            else
            {
                Debug.LogError($"Level file not found: {filePath}");
                onLoaded?.Invoke(null);
            }
#else
            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogError($"Level file not found: {filePath}");
                onLoaded?.Invoke(null);
                yield break;
            }

            var json = System.IO.File.ReadAllText(filePath);
            levelData = JsonUtility.FromJson<LevelData>(json);
            onLoaded?.Invoke(levelData);
#endif
        }
        
        
        
        
        
        
        
        
        
        

        //Wall Builder
        public static readonly string[] PuzzleWallNames = new[]
        {
            "PuzzleWall_Top",
            "PuzzleWall_Bottom",
            "PuzzleWall_Side1",
            "PuzzleWall_Side2",
            "PuzzleWall_Side3",
            "PuzzleWall_Side4"
        };
        
        // To show puzzle objects at exactly same position in each level, we need to build walls by same logic.
        public static void GetPuzzleViewAndWalls(Vector3 baseSize, float topOffset, Vector2 cameraPositionOffset,
            out float cameraFarClipPlane, out Vector3 cameraPosition, out (Vector3 pos, Vector3 scale)[] puzzleWalls)
        {
            // calculate geometry
            float halfWidth = baseSize.x * 0.5f;
            float halfDepth = baseSize.y * 0.5f;
            // Use the larger base dimension as a baseline for height so a square base behaves like the example.
            float height = baseSize.z + topOffset;
            float centerY = topOffset - height * 0.5f;
            const float thin = 0.1f;
            const float bottomThickness = 0.5f;
            
            // calculate camera settings
            cameraFarClipPlane = height;
            cameraPosition = new Vector3(cameraPositionOffset.x, topOffset, cameraPositionOffset.y);
            
            puzzleWalls = new (Vector3 pos, Vector3 scale)[]
            {
                // Top
                (new Vector3(0f, topOffset, 0f), new Vector3(baseSize.x, thin, baseSize.y)),
                // Bottom
                (new Vector3(0f, topOffset - height - bottomThickness / 2, 0f),
                    new Vector3(baseSize.x, bottomThickness, baseSize.y)),
                // Side front (positive Z)
                (new Vector3(0f, centerY, halfDepth + thin / 2), new Vector3(baseSize.x, height, thin)),
                // Side back (negative Z)
                (new Vector3(0f, centerY, -halfDepth - thin / 2), new Vector3(baseSize.x, height, thin)),
                // Side right (positive X)
                (new Vector3(halfWidth + thin / 2, centerY, 0f), new Vector3(thin, height, baseSize.y)),
                // Side left (negative X)
                (new Vector3(-halfWidth - thin / 2, centerY, 0f), new Vector3(thin, height, baseSize.y))
            };
        }
    }
}