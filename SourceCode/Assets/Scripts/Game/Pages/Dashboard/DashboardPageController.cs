using mehmetsrl.MVC.core;

/// <summary>
/// Dashboard page controller.
/// It consists of 3 mvc components called widgets.
/// </summary>
public class DashboardPageController : Controller<DashboardPageView, DashboardPageModel>
{
    public DashboardPageController(DashboardPageModel model) : base(ControllerType.Page, model)
    {
    }

    protected override void OnCreate()
    {
    }

    public void LoadLevel()
    {
        
    }
}