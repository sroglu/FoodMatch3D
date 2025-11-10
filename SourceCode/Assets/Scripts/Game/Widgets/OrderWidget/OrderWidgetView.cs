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
                customerController.View.transform.localPosition = Vector3.zero;
                customerController.View.transform.localRotation = Quaternion.identity;
                customerController.View.transform.localScale = Vector3.one;
                customerController.View.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutBack)
                    .SetLink(customerController.View.gameObject);
                customerController.SetOrder(orderData.OrderId, orderData.Quantity);
            }
        }
        public override void UpdateView()
        {
            //UpdateOrders();
        }
    }
}