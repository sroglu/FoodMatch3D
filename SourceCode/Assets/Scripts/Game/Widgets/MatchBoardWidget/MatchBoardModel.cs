using Game.Data;
using mehmetsrl.MVC.core;

namespace Game.Widgets.MatchWidget
{
    public class MatchBoardModel : Model<PuzzleObjectViewData>
    {
        public MatchBoardModel(PuzzleObjectViewData[] dataArr) : base(dataArr) { }
    }
}