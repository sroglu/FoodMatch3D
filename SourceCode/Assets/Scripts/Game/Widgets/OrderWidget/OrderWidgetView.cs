using System.Collections.Generic;
using Game.DataStores;
using mehmetsrl.MVC.core;
using UnityEngine;

namespace Game.Widgets.OrderWidget
{
    public class OrderWidgetView : View<OrderWidgetModel>
    {
        [SerializeField]
        private RectTransform _slotsParent;

        private readonly Dictionary<int, (RectTransform,CustomerController)> _slots = new();

        protected override void OnCreate()
        {
            Debug.Assert(_slotsParent != null);
            for (int i = 0; i < _slotsParent.childCount; i++)
            {
                RectTransform slot = _slotsParent.GetChild(i) as RectTransform;
                if (slot != null)
                {
                    if(!GameDataStore.Instance.GameData.TryGetCustomerViewData(Model.CurrentDataArr[i].CustomerId, out var customerViewData))
                    {
                        Debug.LogError($"Customer View Data not found for CustomerId: {Model.CurrentDataArr[i].CustomerId}");
                        continue;
                    }

                    var orderData = Model.CurrentDataArr[i];
                    var customerController = InstanceManager.Instance.CreateCustomer(customerViewData);
                    customerController.View.transform.SetParent(slot);
                    customerController.View.transform.localPosition = Vector3.zero;
                    _slots.Add(i, (slot, customerController));
                }
            }
        }

        private void UpdateOrders()
        {
            for (int i = 0; i < Model.OrderCount; i++)
            {
                // Update slot with orderData
                var (slot, customerController) = _slots[i];
                var orderData = Model.CurrentDataArr[i];
                if (orderData.Quantity > 0)
                {
                    customerController.SetOrder(orderData.OrderId, orderData.Quantity);
                }
                else
                {
                    EndOrder(i);
                }
                
            }
        }
        
        private void EndOrder(int indexFrom)
        {
            Model.DecrementOrderCount();
            
            var customerControllerToEnd = _slots[indexFrom].Item2;
            //shift orders up
            for (int j = indexFrom; j < Model.OrderCount; j++)
            {
                var (nextSlot, nextCustomerController) = _slots[j + 1];
                var (currentSlot, currentCustomerController) = _slots[j];
                _slots[j] = (currentSlot, nextCustomerController);
            }

            //clear last slot
            customerControllerToEnd.EndOrder();
        }

        public override void UpdateView()
        {
            UpdateOrders();
        }
    }
}