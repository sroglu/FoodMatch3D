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

            GameDataStore.Instance.SetSlotMatchState(_slots);
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