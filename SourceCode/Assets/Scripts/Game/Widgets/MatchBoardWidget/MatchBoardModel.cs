using Game.Data;
using mehmetsrl.MVC.core;
using UnityEngine;

namespace Game.Widgets.MatchWidget
{
    public class MatchBoardModel : Model<EmptyData>
    {
        public uint SlotCountForMerge { get; private set; }

        public MatchBoardModel(EmptyData data) : base(data) { }
        public void UpdateSlotCountForMergeOnCoinSpend()
        {
            if (SlotCountForMerge - GameData.InitialSlotCountForMerge < GameData.MaxSlotIncrementAmount)
            {
                SlotCountForMerge++;
            }
            else
            {
                Debug.LogError("Max Slot Increment Reached!");
            }
        }
    }
}