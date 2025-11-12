using System;
using Game.Data;
using Game.Instances.ActionButton;
using mehmetsrl.MVC.core;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Widgets.MatchWidget
{
    public class MatchBoardView : View<MatchBoardModel>
    {
        [SerializeField] private RectTransform _matchSlotsParent;
        [SerializeField] private RectTransform _actionButtonParent;
        
        [SerializeField] private Slider _starCollectionStreakSlider;
        [SerializeField] private TMP_Text _starCollectionStreakText;
        
        
        private RectTransform[] _matchSlots;
        private ActionButtonView[] _actionButtons;
        
        public ActionButtonView[] ActionButtons => _actionButtons;
        
        private new MatchBoardController Controller => base.Controller as MatchBoardController;


        protected override void OnCreate()
        {
            Debug.Assert(_matchSlotsParent != null);
            Debug.Assert(_matchSlotsParent.childCount > 0);
            Debug.Assert(_matchSlotsParent.childCount == GameData.InitialSlotCountForMerge + GameData.MaxSlotIncrementAmount);
            
            _matchSlots = new RectTransform[_matchSlotsParent.childCount];
            
            for (var i = 0; i < _matchSlotsParent.childCount; i++)
            {
                RectTransform slot = _matchSlotsParent.GetChild(i) as RectTransform;
                if (slot != null)
                {
                    _matchSlots[i] = slot;
                }
            }
            
            _actionButtons = new ActionButtonView[_actionButtonParent.childCount];
            for (var j = 0; j < _actionButtonParent.childCount; j++)
            {
                var  actionButtonView = _actionButtonParent.GetChild(j).GetComponentInChildren<ActionButtonView>();
                Debug.Assert(actionButtonView != null, $"ActionButtonView not found in {_actionButtonParent.GetChild(j).name}");
                _actionButtons[j] = actionButtonView;
            }
        }

        protected override void OnStateChanged(ViewState state)
        {
            switch (state)
            {
                case ViewState.Visible:
                    Controller.OnViewEnabled();
                    break;
                case ViewState.Invisible:
                    Controller.OnViewDisabled();
                    break;
            }
        }

        public override void UpdateView()
        {
        }
        
        public int MatchSlotLimit => _matchSlots.Length;

        public Vector3 GetSlotRectPosition(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _matchSlots.Length)
            {
                Debug.LogError($"Slot index {slotIndex} is out of bounds.");
                return Vector3.zero;
            }

            return _matchSlots[slotIndex].position;
        }
        
        private float _strikeSliderValue = 0f;

        private void Update()
        {
            Controller.OnUpdateLoop();
            
            _starCollectionStreakSlider.SetValueWithoutNotify(math.lerp(_starCollectionStreakSlider.value, _strikeSliderValue, Time.deltaTime * 5f));
        }

        public void UpdateStarCollectionStreak(byte starCollectionStreak, float strikeSliderValue)
        {
            _strikeSliderValue = strikeSliderValue;
            
            _starCollectionStreakText.text = $"x{starCollectionStreak}";
        }
    }
}