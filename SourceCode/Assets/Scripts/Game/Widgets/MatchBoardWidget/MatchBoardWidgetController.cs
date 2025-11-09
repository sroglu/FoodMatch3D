using mehmetsrl.MVC.core;

namespace Game.Widgets.MatchWidget
{
    public class MatchBoardWidgetController :Controller<MatchBoardView, MatchBoardModel>
    {
        public MatchBoardWidgetController(ControllerType controllerType, MatchBoardModel model, MatchBoardView view = null) : base(controllerType, model, view)
        {
        }
    }
}