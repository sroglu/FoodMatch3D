using Game.Widgets.MatchWidget;
using Game.Widgets.OrderWidget;
using UnityEngine;
using mehmetsrl.MVC.core;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class GamePageView : View<GamePageModel>
{
    [SerializeField] private OrderWidgetView _orderWidgetView;
    [SerializeField] private MatchBoardView _matchBoardView;
    
    [SerializeField] private Image _raycastBlockerImage;
    
    private new GamePageController Controller => base.Controller as GamePageController;
    public OrderWidgetView OrderWidgetView => _orderWidgetView;
    public MatchBoardView MatchBoardView => _matchBoardView;
    public override void UpdateView() { }

    protected override void OnCustomInputAction(CustomActionEventType actionType, InputAction.CallbackContext evArgs,
        GameObject targetObj)
    {
        switch (actionType)
        {
            case CustomActionEventType.Click:
                if (targetObj == _raycastBlockerImage.gameObject)
                {
                    Controller.OnRaycastBlockerClicked();
                }

                break;
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
}
