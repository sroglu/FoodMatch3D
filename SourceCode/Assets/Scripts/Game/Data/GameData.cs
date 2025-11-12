using System;
using System.Collections.Generic;
using Game.Instances.PuzzleInstances;
using UnityEngine;

namespace Game.Data
{
    

    [Serializable]
    public class PuzzleObjectViewData : ICloneable
    {
        [HideInInspector] public string Name;
        public uint TypeId;
        public PuzzleObjectInstance Prefab;
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
        public string DisplayName;
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

        #region Puzzle Constants

        public static readonly Vector3 BaseSize = new Vector3(1.75f, 1.8f, 1f);
        public static readonly float TopOffset = 1f;
        public static readonly Vector2 CameraPositionOffset = new Vector2(0f, -0.2f);

        #endregion
        
        public static byte StarGainStreakWaitDurationInSeconds = 1;
        public static byte StarGainStreakDurationInSeconds = 5;
        public static readonly uint InitialSlotCountForMerge = 7;
        public static readonly uint MaxSlotIncrementAmount = 1;
        public static readonly int MatchCountToClear = 3;
        public static int LastLevelId = 5;
        
        [Header("Rewards"), Space(5)] 
        [SerializeField] 
        private uint _levelCompleteRewardCoins = 100;
        [SerializeField] 
        private uint _keepPlayingCostCoins = 500;
        
        [Header("Puzzle Objects"), Space(5)] 
        [SerializeField] 
        private PuzzleObjectViewData[] _puzzleObjects;
        [SerializeField] 
        private float _puzzleObjectMatchScaleMultiplier = 0.5f;
        [SerializeField] 
        private float _puzzleObjectMatchJumpDuration = 0.2f;
        [SerializeField] 
        private float _puzzleObjectMatchJumpHeight = 0.2f;
        [SerializeField] 
        private float _puzzleObjectMatchUpDuration = 0.2f;        
        [SerializeField] 
        private float _puzzleObjectMatchMoveCenterDistanceOnMatch = 0.3f;
        
        [Header("Customers"), Space(5)] 
        [SerializeField] 
        private CustomerViewData[] _customers;

        #region Getters

        #region Rewards
        
        public uint LevelCompleteRewardCoins => _levelCompleteRewardCoins;
        public uint KeepPlayingCostCoins => _keepPlayingCostCoins;

        #endregion

        #region Puzzle Objects
        public float PuzzleObjectMatchScaleMultiplier => _puzzleObjectMatchScaleMultiplier;
        public float PuzzleObjectMatchJumpDuration => _puzzleObjectMatchJumpDuration;
        public float PuzzleObjectMatchJumpHeight => _puzzleObjectMatchJumpHeight;
        public float PuzzleObjectMatchUpDuration => _puzzleObjectMatchUpDuration;
        public float PuzzleObjectMatchMoveCenterDistanceOnMatch => _puzzleObjectMatchMoveCenterDistanceOnMatch;
        
        #endregion

        #endregion
        
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
        
        
        public uint GetRandomCustomerTypeId()
        {
            if (_customers.Length == 0)
                throw new InvalidOperationException("No customers available in GameData.");

            var randomIndex = UnityEngine.Random.Range(0, _customers.Length);
            return _customers[randomIndex].TypeId;
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