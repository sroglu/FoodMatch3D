using mehmetsrl.MVC.core;

namespace Game.Instances.ActionButton
{
    public class ActionButtonController : Controller<ActionButtonView, ActionButtonModel>
    {
        public ActionButtonController(ActionButtonModel model, ActionButtonView view = null) : base(
            ControllerType.Instance, model, view)
        {
        }
    }
}