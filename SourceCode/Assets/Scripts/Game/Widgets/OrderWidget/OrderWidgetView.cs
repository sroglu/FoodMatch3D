using mehmetsrl.MVC.core;
using TMPro;
using UnityEngine;

namespace Game.Widgets.OrderWidget
{
    public class OrderWidgetView : View<OrderWidgetModel>
    {
        [SerializeField]
        private RectTransform _slotsParent;
        
        [SerializeField] private TMP_Text _timeLimitText;
        
        public int OrderLimitAtTheSameTime => _slotsParent.childCount;
        
        /*private readonly Dictionary<int,CustomerController> _instantiatedCustomers = new();*/
        private new OrderWidgetController Controller => base.Controller as OrderWidgetController;

        protected override void OnCreate()
        {
            Debug.Assert(_slotsParent != null);
        }

        public RectTransform GetSlotRectTransform(int slotIndex)
        {
            return _slotsParent.GetChild(slotIndex) as RectTransform;
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

        public override void UpdateView(){ }
        
        public void UpdateTimer(string getRemainingLevelTime)
        {
            _timeLimitText.text = getRemainingLevelTime;
        }
    }
}