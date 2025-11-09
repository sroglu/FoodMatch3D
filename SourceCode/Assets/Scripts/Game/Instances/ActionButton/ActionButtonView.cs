using Game.Data;
using mehmetsrl.MVC.core;
using UnityEngine;

namespace Game.Instances.ActionButton
{
    public class ActionButtonView : View<ActionButtonModel>
    {
        [SerializeField] private GameAction _gameAction;
        public GameAction GameAction => _gameAction;
        public override void UpdateView()
        {
        }
    }
}