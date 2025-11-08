using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{

    [Serializable]
    public class PuzzleObjectViewData
    {
        [HideInInspector] public string Name;
        public uint TypeId;
        public GameObject Prefab;
        public Sprite Sprite;
    }
    
    [CreateAssetMenu(fileName = nameof(GameData), menuName = "Game/GameData", order = 1)]
    public class GameData : ScriptableObject
    {
        [Header("Levels"), Space(5)] [SerializeField]
        private int _lastLevelId = 5;
        
        [Header("Puzzle Objects"), Space(5)] [SerializeField] 
        private PuzzleObjectViewData[] _puzzleObjects;
        
        
        public PuzzleObjectViewData GetPuzzleObjectViewData(uint typeId)
        {
            foreach (var puzzleObject in _puzzleObjects)
            {
                if (puzzleObject.TypeId == typeId)
                {
                    return puzzleObject;
                }
            }

            Debug.LogError($"PuzzleObjectViewData with TypeId {typeId} not found.");
            return null;
        }


        private void OnValidate()
        {
            if (_puzzleObjects == null) return;

            // Check for duplicate TypeIds and set names
            var typeIds = new HashSet<uint>();
            foreach (var puzzleObject in _puzzleObjects)
            {
                if (puzzleObject == null) continue;

                // Ensure Name follows the pattern "PuzzleObject {TypeId}"
                var newName = $"PuzzleObject {puzzleObject.TypeId}";
                if (puzzleObject.Name != null && puzzleObject.Name != newName)
                {
                    puzzleObject.Name = newName;
                }

                if (!typeIds.Add(puzzleObject.TypeId))
                {
                    Debug.LogError($"Duplicate TypeId found: {puzzleObject.TypeId} in GameData '{name}'");
                }
            }
        }
    }
}