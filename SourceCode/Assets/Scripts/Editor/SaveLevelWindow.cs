using Game.Data;
using UnityEditor;
using UnityEngine;


namespace Game.Editor
{
    public class SaveLevelWindow : EditorWindow
    {
        private static LevelData _levelToSave;
        private static int _levelIndex;
        private static Vector2 _scrollPosition;


        public static void ShowWindow(LevelData levelData)
        {
            _levelToSave = levelData;
            GetWindow<SaveLevelWindow>(true, "Save Level");
        }

        private void OnDisable()
        {
            _levelToSave = null;
        }

        private void OnGUI()
        {
            GUILayout.Label("Save Level Options", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Implement your level saving logic here. This could involve choosing a file path and serializing grid data.",
                MessageType.Info);
            
            System.IO.Directory.CreateDirectory(
                Constants.LevelPaths.STREAMING_ASSETS_PATH + $"/{Constants.LevelPaths.LEVEL_FOLDER}");
            
            // get all level files 
            var levelFiles = System.IO.Directory.GetFiles(
                Constants.LevelPaths.STREAMING_ASSETS_PATH + $"/{Constants.LevelPaths.LEVEL_FOLDER}", "level_*.json");

            
            _levelIndex = _levelToSave.Id.Value;
            if (levelFiles.Length > 0)
            {
                GUILayout.Label("Existing Levels:");
                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Height(200));
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
                foreach (var file in levelFiles)
                {
                    if (GUILayout.Button(System.IO.Path.GetFileName(file).Split(".")[0]))
                    {
                        var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                        if (int.TryParse(fileName.Replace("level_", ""), out var index))
                            _levelIndex = index;
                    }
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
            else
            {
                GUILayout.Label("No existing levels found.");
            }

            _levelIndex = EditorGUILayout.IntField("Level Index", _levelIndex);

            if (GUILayout.Button("Save Level"))
            {
                SaveLevel();
            }
        }

        private void SaveLevel()
        {
            // if level exists, ask for confirmation
            if (System.IO.File.Exists(
                    $"{Constants.LevelPaths.STREAMING_ASSETS_PATH}/{Constants.LevelPaths.LEVEL_FOLDER}/level_{_levelIndex}.json"))
            {
                if (!EditorUtility.DisplayDialog("Overwrite Level",
                        $"Level {_levelIndex} already exists. Do you want to overwrite it?", "Yes", "No"))
                {
                    return; // User chose not to overwrite
                }
            }

            _levelToSave.Id = new LevelId(_levelIndex);
            var levelToSave = JsonUtility.ToJson(_levelToSave);
            // write level to streamed assets path
            string filePath =
                $"{Constants.LevelPaths.STREAMING_ASSETS_PATH}/{Constants.LevelPaths.LEVEL_FOLDER}/level_{_levelIndex}.json";
            System.IO.File.WriteAllText(filePath, levelToSave);
            Debug.Log($"Level saved to {filePath}");
            EditorUtility.DisplayDialog("Level Saved", $"Level {_levelIndex} has been saved successfully!", "OK");
            //AssetDatabase.SaveAssetIfDirty();
            AssetDatabase.Refresh();
            // Close the window after saving
            Close();
        }
    }
}