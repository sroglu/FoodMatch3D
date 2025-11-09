using Game.Data;
using mehmetsrl.MVC.core;

namespace Game.Instances.ActionButton
{
    public class ActionButtonModel :Model<GameActionData>
    {
        public ActionButtonModel(GameActionData data) : base(data) { }
    }
}