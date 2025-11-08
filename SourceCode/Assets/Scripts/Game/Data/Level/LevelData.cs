using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    [Serializable]
    public class Level
    {
        [HideInInspector] public string Name;
        public LevelId Id;
        public PuzzleObject[] PuzzleObjects;
    }

    [Serializable]
    public class PuzzleObject
    {
        [HideInInspector] public string Name;
        public uint TypeId;
        public uint Quantity;
    }

    [CreateAssetMenu(fileName = nameof(LevelData), menuName = "Game/LevelData", order = 1)]
    public class LevelData : ScriptableObject
    {
        [SerializeField] private Level[] _levels;

        public bool TryGetLevelById(LevelId levelId, out Level level)
        {
            foreach (var lvl in _levels)
            {
                if (lvl.Id.Value == levelId.Value)
                {
                    level = lvl;
                    return true;
                }
            }

            level = null;
            return false;
        }
        
        
        private void OnValidate()
        {
            if (_levels == null) return;

            // Check for duplicate LevelIds and set names
            var levelIds = new HashSet<int>();
            foreach (var level in _levels)
            {
                if (level == null) continue;

                // Ensure Name follows the pattern "Level {Id}"
                var newName = $"Level {level.Id.Value}";
                if (level.Name != null && level.Name != newName)
                {
                    level.Name = newName;
                }

                // Ensure each PuzzleObject has a name like "Puzzle {TypeId} x{Quantity}"
                if (level.PuzzleObjects != null)
                {
                    foreach (var p in level.PuzzleObjects)
                    {
                        if (p == null) continue;
                        var newPuzzleName = $"PuzzleObject {p.TypeId} x {p.Quantity}";
                        if (p.Name != newPuzzleName)
                        {
                            p.Name = newPuzzleName;
                        }
                    }
                }

                if (!levelIds.Add(level.Id.Value))
                {
                    Debug.LogError($"Duplicate LevelId found: {level.Id.Value} in LevelData '{name}'");
                }
            }

            // Check for missing LevelIds
            for (int i = 0; i < _levels.Length; i++)
            {
                if (!levelIds.Contains(i))
                {
                    Debug.LogError($"Missing LevelId: {i} in LevelData '{name}'");
                }
            }
        }
    }
}