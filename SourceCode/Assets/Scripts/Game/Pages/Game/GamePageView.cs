using Game.Widgets.MatchWidget;
using Game.Widgets.OrderWidget;
using UnityEngine;
using mehmetsrl.MVC.core;


public class GamePageView : View<GamePageModel>
{
    [SerializeField] private OrderWidgetView _orderWidgetView;
    [SerializeField] private MatchBoardView _matchBoardView;
    
    public OrderWidgetView OrderWidgetView => _orderWidgetView;
    public MatchBoardView MatchBoardView => _matchBoardView;
    public override void UpdateView() { }
}
