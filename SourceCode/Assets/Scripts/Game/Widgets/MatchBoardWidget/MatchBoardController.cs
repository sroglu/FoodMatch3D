using System.Collections.Generic;
using DG.Tweening;
using Game.Data;
using Game.DataStores;
using Game.Instances.ActionButton;
using Game.Instances.PuzzleInstances;
using mehmetsrl.MVC.core;
using UnityEngine;

namespace Game.Widgets.MatchWidget
{
    public class MatchBoardController : Controller<MatchBoardView, MatchBoardModel>
    {
        private readonly Dictionary<ActionButtonView, ActionButtonController> _actionButtonControllers = new();

        private readonly List<PuzzleObjectInstance> _slots = new();
        private readonly List<int> _puzzleObjectIndicesToMatchCheck = new(2);
        private readonly List<int> _buckedToSelectUnmatches = new();
        private readonly PuzzleObjectInstance[] _puzzleObjectsToBeMatched = new PuzzleObjectInstance[GameData.MatchCountToClear];

        public MatchBoardController(MatchBoardModel model, MatchBoardView view = null) : base(ControllerType.Instance,
            model, view)
        {
        }

        protected override void OnCreate()
        {
            foreach (var actionButtonView in View.ActionButtons)
            {
                if (actionButtonView.GameAction != GameAction.None)
                {
                    var actionButtonModel = new ActionButtonModel(new GameActionData(actionButtonView.GameAction));
                    var actionButtonController = new ActionButtonController(actionButtonModel, actionButtonView);
                    _actionButtonControllers.Add(actionButtonView, actionButtonController);
                }
            }
        }

        private bool GetInsertIndex(uint typeId, out int insertIndex)
        {
            var lastIndex = -1;
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].TypeId == typeId) lastIndex = i;
            }
            
            insertIndex = lastIndex >= 0 ? lastIndex + 1 : _slots.Count;
            return insertIndex < View.MatchSlotLimit;
        }

        public void AddToMatchBoard(PuzzleObjectInstance puzzleObjectInstance)
        {
            if (puzzleObjectInstance == null) return;

            if(!GetInsertIndex(puzzleObjectInstance.TypeId, out int insertIndex))
            {
                //No space to insert end the game
                Debug.Log("No space to insert the puzzle object. Game Over.");
                GameManager.Instance.CompleteLevel(false);
                return;
            }
            
            _slots.Insert(insertIndex, puzzleObjectInstance);

            CheckAndHandleMatches();
            
            //Move other slots to their new positions
            for (int i = 0; i < _slots.Count; i++)
            {
                if (insertIndex == i)
                {
                    MovePuzzleObjectToViewSlot(i);
                }
                else
                {
                    MovePuzzleObjectInViewSlot(i);
                }
            }

        }


        private void CheckAndHandleMatches()
        {
            _puzzleObjectIndicesToMatchCheck.Clear();
            _buckedToSelectUnmatches.Clear();

            uint lastTypeId = _slots[0].TypeId;
            _puzzleObjectIndicesToMatchCheck.Add(0);

            for (int i = 1; i < _slots.Count; i++)
            {
                if (_slots[i].TypeId == lastTypeId)
                {
                    if(_puzzleObjectIndicesToMatchCheck.Count == 0)
                    {
                        //remove previous from unmatches and add both to match check
                        _buckedToSelectUnmatches.Remove(i - 1);
                        _puzzleObjectIndicesToMatchCheck.Add(i - 1);
                    }
                    _puzzleObjectIndicesToMatchCheck.Add(i);
                    if (_puzzleObjectIndicesToMatchCheck.Count == GameData.MatchCountToClear)
                    {
                        _puzzleObjectIndicesToMatchCheck.Clear();
                    }
                }
                else
                {
                    _buckedToSelectUnmatches.AddRange(_puzzleObjectIndicesToMatchCheck);
                    _puzzleObjectIndicesToMatchCheck.Clear();
                    _buckedToSelectUnmatches.Add(i);
                }

                lastTypeId = _slots[i].TypeId;
            }

            //clear remaining matches to check as unmatches
            _buckedToSelectUnmatches.AddRange(_puzzleObjectIndicesToMatchCheck);
            _puzzleObjectIndicesToMatchCheck.Clear();


            int matchIndex = 0;
            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                bool shouldRemove = !_buckedToSelectUnmatches.Contains(i);
                if (!shouldRemove) continue;
                
                var puzzleObjectInstance = _slots[i];
                _slots.RemoveAt(i);
                
                _puzzleObjectsToBeMatched[matchIndex++] = puzzleObjectInstance;
                
                if(matchIndex == GameData.MatchCountToClear)
                {
                    HandleMatched((PuzzleObjectInstance[])_puzzleObjectsToBeMatched.Clone());
                    matchIndex = 0;
                }
            }
        }
        
        private void HandleMatched(PuzzleObjectInstance[] matchedPuzzleObjects)
        {
            Debug.Assert(matchedPuzzleObjects.Length == GameData.MatchCountToClear);
                
            GameDataStore.Instance.AddClearMatchedPuzzleObject(matchedPuzzleObjects[0].TypeId);
            
            //scale up the middle and move others to the sides and dispose them
            var middleIndex = matchedPuzzleObjects.Length / 2;
            for (int i = 0; i < matchedPuzzleObjects.Length; i++)
            {
                var puzzleObjectInstance = matchedPuzzleObjects[i];
                if (i == middleIndex)
                {
                    //Scale up animation
                    var targetScale = Vector3.one;
                    puzzleObjectInstance.transform
                        .DOScale(targetScale, GameDataStore.Instance.GameData.PuzzleObjectMatchUpDuration)
                        .SetEase(Ease.OutQuad)
                        .SetLink(puzzleObjectInstance.gameObject).OnComplete(() =>
                        {
                            //Dispose after scaling up
                            puzzleObjectInstance.Dispose();
                            //TODO: Add effects stars or particles here
                        });
                }
                else
                {
                    //Move to sides and dispose
                    var direction = i < middleIndex ? -1 : 1;
                    var targetPosition = puzzleObjectInstance.transform.position +
                                         new Vector3(direction * GameDataStore.Instance.GameData
                                             .PuzzleObjectMatchMoveCenterDistanceOnMatch, 0, 0);
                    puzzleObjectInstance.transform
                        .DOMove(targetPosition, GameDataStore.Instance.GameData.PuzzleObjectMatchUpDuration)
                        .SetEase(Ease.OutQuad)
                        .SetLink(puzzleObjectInstance.gameObject).OnComplete(() =>
                        {
                            //Dispose after moving out
                            puzzleObjectInstance.Dispose();
                        });
                }
            }
        }

        // Move the puzzle object already in the view slot to its new position
        private void MovePuzzleObjectInViewSlot(int slotIndex)
        {
            var puzzleObjectInstance = _slots[slotIndex];
            var targetSlotPosition = GetTargetSlotPosition(slotIndex);
            
            puzzleObjectInstance.transform.DOMove(targetSlotPosition,
                    GameDataStore.Instance.GameData.PuzzleObjectMatchJumpDuration).SetEase(Ease.OutQuad)
                .SetLink(puzzleObjectInstance.gameObject).OnComplete(() =>
                {
                    puzzleObjectInstance.transform.position =
                        puzzleObjectInstance.transform.position;
                });
        }

        // Move the puzzle object to be added to its target view slot
        private void MovePuzzleObjectToViewSlot(int slotIndex)
        {
            var puzzleObjectInstance = _slots[slotIndex];
            var targetSlotPosition = GetTargetSlotPosition(slotIndex);

            var rigidbody = puzzleObjectInstance.GetComponentInChildren<Rigidbody>();
            if (rigidbody == null)
            {
                Debug.LogError("Rigidbody component not found on PuzzleObjectInstance.");
                return;
            }

            rigidbody.isKinematic = true;

            //TODO: Add front view rotation info to show puzzle object facing front here 
            puzzleObjectInstance.transform.DORotateQuaternion(Quaternion.identity,
                    GameDataStore.Instance.GameData.PuzzleObjectMatchJumpDuration).SetEase(Ease.OutQuad)
                .SetLink(puzzleObjectInstance.gameObject).OnComplete(() => { });

            puzzleObjectInstance.transform
                .DOJump(targetSlotPosition, GameDataStore.Instance.GameData.PuzzleObjectMatchJumpHeight, 1,
                    GameDataStore.Instance.GameData.PuzzleObjectMatchJumpDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(puzzleObjectInstance.gameObject).OnComplete(() => { });
            var targetScale = puzzleObjectInstance.transform.localScale *
                              GameDataStore.Instance.GameData.PuzzleObjectMatchScaleMultiplier;
            puzzleObjectInstance.transform
                .DOScale(targetScale, GameDataStore.Instance.GameData.PuzzleObjectMatchJumpDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(puzzleObjectInstance.gameObject).OnComplete(() =>
                {
                    puzzleObjectInstance.transform.localScale = targetScale;
                });
        }

        private Vector3 GetTargetSlotPosition(int slotIndex)
        {
            var targetSlotPosition = View.GetSlotRectPosition(slotIndex);
            var cameraPosition = GameDataStore.Instance.GameCamera.transform.position;
            return new Vector3(targetSlotPosition.x, (targetSlotPosition.y + cameraPosition.y) / 2,
                targetSlotPosition.z);
        }
    }
}