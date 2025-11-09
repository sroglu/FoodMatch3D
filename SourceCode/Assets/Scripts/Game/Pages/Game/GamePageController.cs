using Game.Widgets.MatchWidget;
using Game.Widgets.OrderWidget;
using mehmetsrl.MVC.core;

/// <summary>
/// Game page controller.
/// It consists of 2 mvc components called widgets.
/// </summary>
public class GamePageController : Controller<GamePageView, GamePageModel>
{
    private OrderWidgetController _orderWidgetController;
    private MatchBoardController _matchBoardController;
    
    public GamePageController(GamePageModel model) : base(ControllerType.Page, model)
    {
    }

    protected override void OnCreate()
    {
        _orderWidgetController = new OrderWidgetController(View.OrderWidgetView.Model, View.OrderWidgetView);
        _matchBoardController = new MatchBoardController(View.MatchBoardView.Model, View.MatchBoardView);
    }

    public void LoadLevel()
    {
    }
}