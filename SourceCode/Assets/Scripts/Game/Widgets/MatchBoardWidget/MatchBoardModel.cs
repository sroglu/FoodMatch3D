using Game.Data;
using mehmetsrl.MVC.core;
using UnityEngine;

namespace Game.Widgets.MatchWidget
{
    public class MatchBoardModel : Model<PuzzleObjectViewData>
    {
        public uint SlotCountForMerge { get; private set; }

        public MatchBoardModel(PuzzleObjectViewData[] dataArr) : base(dataArr)
        {
            SlotCountForMerge = GameData.InitialSlotCountForMerge;
        }
        
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