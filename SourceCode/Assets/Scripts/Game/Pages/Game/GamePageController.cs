using mehmetsrl.MVC.core;

/// <summary>
/// Game page controller.
/// It consists of 3 mvc components called widgets.
/// </summary>
public class GamePageController : Controller<GamePageView, GamePageModel>
{
    public GamePageController(GamePageModel model) : base(ControllerType.Page, model)
    {
    }

    protected override void OnCreate()
    {
        InstanceManager.Instance.ClearAllInstances();
    }

    public void LoadLevel()
    {
    }
}