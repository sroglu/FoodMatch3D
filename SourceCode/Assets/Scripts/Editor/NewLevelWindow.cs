using System;
using Game.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public class NewLevelWindow : EditorWindow
    {
        // A static event that the main LevelEditor can subscribe to.
        // This is how we pass the data back.
        public static event Action<LevelData> OnCreateNewLevel;

        // The window will edit its own private copy of the data.
        private LevelData _levelData;

        /// <summary>
        /// Shows the window and initializes it with default data.
        /// </summary>
        public static void ShowWindow()
        {
            NewLevelWindow window = GetWindow<NewLevelWindow>(true, "Create New Level");

            // Initialize with some default values
            window._levelData = new LevelData();
            window.minSize = new Vector2(350, 250);
            window.maxSize = new Vector2(350, 250);
        }

        private void OnGUI()
        {
            GUILayout.Label("New Level Properties", EditorStyles.boldLabel);
            
            _levelData.Id.Value = EditorGUILayout.IntField("Level Id", _levelData.Id.Value);
            //get number of puzzle object types
            int puzzleObjectTypeCount = EditorGUILayout.IntField("Number of Puzzle Object Types", _levelData.PuzzleObjects == null ? 0 : _levelData.PuzzleObjects.Length);
            if (puzzleObjectTypeCount < 0) puzzleObjectTypeCount = 0;
            if (_levelData.PuzzleObjects == null || puzzleObjectTypeCount != _levelData.PuzzleObjects.Length)
            {
                _levelData.PuzzleObjects = new PuzzleObject[puzzleObjectTypeCount];
            }

            EditorGUILayout.Space(10);

            // --- Edit GridData properties ---

            // --- Create Button ---
            if (GUILayout.Button("Create Level"))
            {
                // Re-initialize the LevelData to resize the internal arrays based on the new properties
                var finalData = _levelData;
                // Set level data properties from the edited values
                
                // Invoke the event, sending the new data to any listeners (like our main LevelEditor)
                OnCreateNewLevel?.Invoke(finalData);

                // Close this window
                this.Close();
            }
        }
    }
}