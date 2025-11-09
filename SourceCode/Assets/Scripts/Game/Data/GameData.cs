using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    

    [Serializable]
    public class PuzzleObjectViewData : ICloneable
    {
        [HideInInspector] public string Name;
        public uint TypeId;
        public GameObject Prefab;
        public Sprite Sprite;
        public object Clone()
        {
            return MemberwiseClone();
        }
    } 

    [Serializable]
    public class CustomerViewData : ICloneable
    {
        [HideInInspector] public string Name;
        public uint TypeId;
        public Sprite Sprite;
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
    
    [CreateAssetMenu(fileName = nameof(GameData), menuName = "Game/GameData", order = 1)]
    public class GameData : ScriptableObject
    {
        public static readonly int MatchCountToClear = 3;
        public static int LastLevelId = 5;
        
        [Header("Puzzle Objects"), Space(5)] [SerializeField] 
        private PuzzleObjectViewData[] _puzzleObjects;
        
        [Header("Customers"), Space(5)] [SerializeField] 
        private CustomerViewData[] _customers;
        
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
        
        public bool TryGetCustomerViewData(uint typeId, out CustomerViewData customerViewData)
        {
            foreach (var customer in _customers)
            {
                if (customer.TypeId == typeId)
                {
                    customerViewData = customer;
                    return true;
                }
            }

            customerViewData = null;
            return false;
        }


        private void OnValidate()
        {
            var typeIds = new HashSet<uint>();
            
            // Check PuzzleObjects for duplicates and correct naming
            if (_puzzleObjects == null) return;
            // Check for duplicate TypeIds and set names
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
            
            //Check Customers for duplicates and correct naming
            typeIds.Clear();
            if (_customers == null) return;
            // Check for duplicate TypeIds and set names
            foreach (var customer in _customers)
            {
                if (customer == null) continue;
                // Ensure Name follows the pattern "Customer {TypeId}"
                var newName = $"Customer {customer.TypeId}";
                if (customer.Name != null && customer.Name != newName)
                {
                    customer.Name = newName;
                }
                if (!typeIds.Add(customer.TypeId))
                {
                    Debug.LogError($"Duplicate TypeId found: {customer.TypeId} in GameData '{name}'");
                }
            }
        }
    }
}