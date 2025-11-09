using System.Collections.Generic;
using DG.Tweening;
using Game.Data;
using Game.DataStores;
using mehmetsrl.MVC.core;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Widgets.OrderWidget
{
    public class OrderWidgetView : View<OrderWidgetModel>
    {
        [SerializeField]
        private RectTransform _slotsParent;

        
        public int OrderLimitAtTheSameTime => _slotsParent.childCount;
        
        private readonly Dictionary<int,CustomerController> _instantiatedCustomers = new();

        protected override void OnCreate()
        {
            Debug.Assert(_slotsParent != null);
        }

        /*public void LoadOrders(OrderData[] orderDataArr)
        {
            for (int i = 0; i < orderDataArr.Length; i++)
            {
                var orderData = orderDataArr[i];
                Debug.Assert(!orderData.IsCompleted);
                
                if (!GameDataStore.Instance.GameData.TryGetCustomerViewData(orderData.CustomerId,
                        out var customerViewData))
                {
                    Debug.LogError($"Customer View Data not found for CustomerId: {orderData.CustomerId}");
                    continue;
                }

                RectTransform slot = _slotsParent.GetChild(i) as RectTransform;
                var customerController = InstanceManager.Instance.CreateCustomer(customerViewData);
                customerController.View.transform.SetParent(slot);
                customerController.View.transform.localPosition = Vector3.zero;
                customerController.SetOrder(orderData.OrderId, orderData.Quantity);
            }
        }*/

        public void UpdateOrders((int,OrderData)[] orderDataArr)
        {
            for (int i = 0; i < orderDataArr.Length; i++)
            {
                var (orderIndex, orderData) = orderDataArr[i];
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
                customerController.View.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutBack)
                    .SetLink(customerController.View.gameObject);
                customerController.SetOrder(orderData.OrderId, orderData.Quantity);
            }
        }
     /*   
        private void UpdateOrdersSlots(){
            for (int i = 0; i < Model.OrderCount; i++)
            {
                var (slot, customerController) = _slots[i];
                customerController.View.transform.SetParent(slot);
                customerController.View.transform.localPosition = Vector3.zero;
            }
        }

        private void UpdateOrders()
        {
            bool anyOrderEnded = false;
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
                    anyOrderEnded = true;
                }
            }

            if (anyOrderEnded)
            {
                UpdateOrdersSlots();
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
*/
        public override void UpdateView()
        {
            //UpdateOrders();
        }
    }
}