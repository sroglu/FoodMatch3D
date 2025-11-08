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
        public static readonly int MatchCountToClear = 3;
        public static int LastLevelId = 5;
        
        [Header("Puzzle Objects"), Space(5)] [SerializeField] 
        private PuzzleObjectViewData[] _puzzleObjects;
        
        public bool TryGetPuzzleObjectViewData(uint typeId, out PuzzleObjectViewData puzzleObjectViewData)
        {
            foreach (var puzzleObject in _puzzleObjects)
            {
                if (puzzleObject.TypeId == typeId)
                {
                    puzzleObjectViewData = puzzleObject;
                    return true;
                }
            }

            puzzleObjectViewData = null;
            return false;
        }
        
        public uint[] GetAllPuzzleObjectTypeIds()
        {
            var typeIds = new uint[_puzzleObjects.Length];
            for (int i = 0; i < _puzzleObjects.Length; i++)
            {
                typeIds[i] = _puzzleObjects[i].TypeId;
            }
            return typeIds;
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