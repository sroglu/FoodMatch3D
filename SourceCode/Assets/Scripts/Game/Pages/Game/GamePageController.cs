using System;
using Game.Data;
using Game.DataStores;
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
        var orderCount = 0;
        var orderData = new OrderData[Model.CurrentData.Level.PuzzleObjects.Length];
        for (int i = 0; i < orderData.Length; i++)
        {
            var existingPuzzleObject = Model.CurrentData.Level.PuzzleObjects[i];
            
            if(!existingPuzzleObject.IsOrdered) continue;
            
            //Get random customer type id for this puzzle object without using GameDataStore
            var customerTypeId = GameDataStore.Instance.GameData.GetRandomCustomerTypeId();
            orderData[orderCount] = new OrderData(customerTypeId, existingPuzzleObject.TypeId, existingPuzzleObject.Quantity);
            orderCount++;
        }
        Array.Resize(ref orderData, orderCount);
        
        _orderWidgetController = new OrderWidgetController(new OrderWidgetModel(orderData), View.OrderWidgetView);
        _matchBoardController = new MatchBoardController(View.MatchBoardView.Model, View.MatchBoardView);
    }
    
}