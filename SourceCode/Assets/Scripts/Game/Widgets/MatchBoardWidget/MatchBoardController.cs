using System.Collections.Generic;
using Game.Data;
using Game.DataStores;
using Game.Instances.ActionButton;
using mehmetsrl.MVC.core;

namespace Game.Widgets.MatchWidget
{
    public class MatchBoardController :Controller<MatchBoardView, MatchBoardModel>
    {
        private readonly Dictionary<ActionButtonView, ActionButtonController> _actionButtonControllers = new();
        
        private readonly List<PuzzleObjectInstanceData> _slots = new();
        
        public MatchBoardController(MatchBoardModel model, MatchBoardView view = null) : base(ControllerType.Instance, model, view)
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
        

        public void AddToMatchBoard(PuzzleObjectInstanceData puzzleObjectInstanceData)
        {
            if (puzzleObjectInstanceData == null) return;

            // Find last index of same type to insert next to existing block
            int lastIndex = -1;
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].TypeId == puzzleObjectInstanceData.TypeId) lastIndex = i;
            }

            int insertIndex = lastIndex >= 0 ? lastIndex + 1 : _slots.Count;
            _slots.Insert(insertIndex, puzzleObjectInstanceData);

            // Scan contiguous block of same type around the inserted index
            int left = insertIndex;
            while (left - 1 >= 0 && _slots[left - 1].TypeId == puzzleObjectInstanceData.TypeId) left--;

            int right = insertIndex;
            while (right + 1 < _slots.Count && _slots[right + 1].TypeId == puzzleObjectInstanceData.TypeId) right++;

            int matchCount = right - left + 1;
            if (matchCount >= GameData.MatchCountToClear)
            {
                _slots.RemoveRange(left, matchCount);
            }
            
            GameDataStore.Instance.SetSlotState(_slots);
            View.UpdateView();
        }

    }
}