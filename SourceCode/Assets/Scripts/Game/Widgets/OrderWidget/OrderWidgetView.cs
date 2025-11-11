using System.Collections.Generic;
using mehmetsrl.MVC.core;
using UnityEngine;

namespace Game.Widgets.OrderWidget
{
    public class OrderWidgetView : View<OrderWidgetModel>
    {
        [SerializeField]
        private RectTransform _slotsParent;

        
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

        /*public void UpdateOrders((int,OrderData)[] ordersToBeDisplayed)
        {
            for (int i = 0; i < ordersToBeDisplayed.Length; i++)
            {
                var (orderIndex, orderData) = ordersToBeDisplayed[i];
                Debug.Assert(!orderData.IsCompleted);
                
                if (!GameDataStore.Instance.GameData.TryGetCustomerViewData(orderData.CustomerId,
                        out var customerViewData))
                {
                    Debug.LogError($"Customer View Data not found for CustomerId: {orderData.CustomerId}");
                    continue;
                }

                RectTransform slot = _slotsParent.GetChild(i) as RectTransform;

                if (!_instantiatedCustomers.TryGetValue(orderIndex, out var customerController))
                {
                    customerController = InstanceManager.Instance.CreateCustomer(customerViewData);
                }
                
                customerController.View.transform.SetParent(slot);
                customerController.View.transform.localPosition = Vector3.zero;
                customerController.View.transform.localRotation = Quaternion.identity;
                customerController.View.transform.localScale = Vector3.one;
                customerController.View.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutBack)
                    .SetLink(customerController.View.gameObject);
                customerController.SetOrder(orderData.OrderId, orderData.Quantity);
            }
        }*/

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

        public override void UpdateView()
        {
            //UpdateOrders();
        }
    }
}