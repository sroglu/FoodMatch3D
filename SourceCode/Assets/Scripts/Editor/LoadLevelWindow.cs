using System;
using Game.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public class LoadLevelWindow : EditorWindow
    {
        private static Vector2 _scrollPosition;
        private Vector2 _existingLevelsScrollPosition;
        private Vector2 _loopLevelsScrollPosition;
        public static event Action<LevelData> LevelLoaded;

        public static void ShowWindow()
        {
            // Get existing open window or if none, make a new one.
            // The 'true' argument makes it a utility window, which doesn't need to be docked.
            GetWindow<LoadLevelWindow>(true, "Load Level");
        }

        private void OnGUI()
        {
            GUILayout.Label("Load Level Options", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Implement your level loading logic here. This could involve file selection, parsing JSON/XML, etc.",
                MessageType.Info);

            var levelFolderPath =
                $"{Constants.LevelPaths.STREAMING_ASSETS_PATH}/{Constants.LevelPaths.LEVEL_FOLDER}/";
            
            System.IO.Directory.CreateDirectory(levelFolderPath);

            var levelFiles = System.IO.Directory.GetFiles(levelFolderPath,"level_*.json");

            if (levelFiles.Length > 0)
            {
                GUILayout.Label("Existing Levels:");
                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Height(200));
                _existingLevelsScrollPosition = EditorGUILayout.BeginScrollView(_existingLevelsScrollPosition);
                foreach (var file in levelFiles)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(System.IO.Path.GetFileName(file).Split(".")[0]))
                    {
                        if (System.IO.File.Exists(file))
                        {
                            var levelId =
                                new LevelId(
                                    int.Parse((System.IO.Path.GetFileNameWithoutExtension(file)).Split('_')[1]));
                            var levelData = LevelUtils.LoadLevel(levelId);
                            LevelLoaded?.Invoke(levelData);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
            else
            {
                GUILayout.Label("No existing levels found.");
            }
        }
    }
}