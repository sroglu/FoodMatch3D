using System.Collections.Generic;
using DG.Tweening;
using Game.Data;
using Game.DataStores;
using mehmetsrl.MVC.core;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Widgets.OrderWidget
{
    public class OrderWidgetController : Controller<OrderWidgetView, OrderWidgetModel>
    {
        private readonly Dictionary<int,CustomerController> _instantiatedCustomers = new();
        private readonly List<int> _ordersIndexes = new();
        public OrderWidgetController(OrderWidgetModel model, OrderWidgetView view = null) : base(ControllerType.Instance, model, view) { }

        protected override void OnCreate()
        {
            SetupInitialOrders();
        }
        
        private void SetupInitialOrders()
        {
            for (int i = 0; i < Model.CurrentDataArr.Length; i++)
            {
                _ordersIndexes.Add(i);
            }
            FilterAndDisplayOrders();
        }

        public void OnViewEnabled()
        {
            GameDataStore.Instance.OnPuzzleObjectMatched += OnPuzzleObjectMatched;
        }

        public void OnViewDisabled()
        {
            GameDataStore.Instance.OnPuzzleObjectMatched -= OnPuzzleObjectMatched;
            DestroyAllCustomers();
        }

        private void DestroyAllCustomers()
        {
            foreach (var customerController in _instantiatedCustomers.Values)
            {
                customerController.Dispose();
            }
            _instantiatedCustomers.Clear();
        }

        private void OnPuzzleObjectMatched()
        {
            if (!GameDataStore.Instance.TryConsumeMatchAction(out var matchedPuzzleObjects))
            {
                Debug.LogError("Failed to consume match action.");
                return;
            }
            FindAndReduceOrders(matchedPuzzleObjects);
        }

        private void FindAndReduceOrders(uint matchedPuzzleObjects)
        {
            foreach (var orderIndex in _ordersIndexes)
            {
                var orderData = Model.CurrentDataArr[orderIndex];
                if (orderData.IsCompleted) continue;

                Debug.Assert(orderData.Quantity > 0);

                if (orderData.OrderId == matchedPuzzleObjects)
                {
                    orderData.Quantity --;
                    Debug.Log($"Order {orderIndex} updated. Remaining Quantity: {orderData.Quantity}");
                    
                    if (orderData.Quantity == 0)
                    {
                        
                        if(!_instantiatedCustomers.TryGetValue(orderIndex, out var customerController))
                        {
                            Debug.LogError($"CustomerController not found for orderIndex: {orderIndex}");
                            continue;
                        }
                        //Update to clear order visually when scaling down
                        //customerController.SetOrder(orderData.OrderId, orderData.Quantity);
                        customerController.View.RectTransform.DOScale(Vector3.zero, 0.3f).OnComplete(() =>
                        {
                            orderData.IsCompleted = true;
                            customerController.EndOrder();
                            _instantiatedCustomers.Remove(orderIndex);
                            UpdateOrders();
                            
                        }).SetLink(customerController.View.gameObject);
                        
                    }
                }
            }

            UpdateOrders();
        }
        
        private void UpdateOrders()
        {
            //Update orders and remove completed ones
            for (int i = 0; i < _ordersIndexes.Count; i++)
            {
                var orderIndex = _ordersIndexes[i];
                var orderData = Model.CurrentDataArr[orderIndex];
                if (orderData.IsCompleted)
                {
                    Model.DecrementOrderCount();
                    _ordersIndexes.RemoveAt(i);
                    i--;
                }
            }

            FilterAndDisplayOrders();
            
            if (_ordersIndexes.Count == 0)
            {
                //Complete level
                Debug.Log("All orders completed!");
                GameManager.Instance.CompleteLevel(true);
            }
        }
        
        private void FilterAndDisplayOrders()
        {
            var ordersToBeDisplayed = new int[math.clamp(_ordersIndexes.Count, 0, View.OrderLimitAtTheSameTime)];
            for (int i = 0; i < ordersToBeDisplayed.Length; i++)
            {
                ordersToBeDisplayed[i] = _ordersIndexes[i];
            }
            ShowOrders(ordersToBeDisplayed);
        }

        private void ShowOrders(int[] ordersToBeDisplayed)
        {
            for (int i = 0; i < ordersToBeDisplayed.Length; i++)
            {
                var orderIndex= ordersToBeDisplayed[i];
                var orderData = Model.CurrentDataArr[orderIndex];
                Debug.Assert(!orderData.IsCompleted);

                if (!GameDataStore.Instance.GameData.TryGetCustomerViewData(orderData.CustomerId,
                        out var customerViewData))
                {
                    Debug.LogError($"Customer View Data not found for CustomerId: {orderData.CustomerId}");
                    continue;
                }

                var slot = View.GetSlotRectTransform(i);

                if (!_instantiatedCustomers.TryGetValue(orderIndex, out var customerController))
                {
                    customerController = InstanceManager.Instance.CreateCustomer(customerViewData);
                    _instantiatedCustomers.Add(orderIndex, customerController);
                }
                
                
                customerController.View.RectTransform.SetParent(slot);
                
                customerController.View.RectTransform.sizeDelta = Vector2.zero;
                customerController.View.RectTransform.anchorMin = Vector2.zero;
                customerController.View.RectTransform.anchorMax = Vector2.one;
                customerController.View.RectTransform.anchoredPosition = Vector2.zero;
                
                
                customerController.View.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutBack)
                    .SetLink(customerController.View.gameObject);
                customerController.SetOrder(orderData.OrderId, orderData.Quantity);
            }
            
        }

        public void Update(OrderData[] getOrdersFromLevelData)
        {
            Model.Update(getOrdersFromLevelData);
            foreach(var orderData in _instantiatedCustomers)
            {
                orderData.Value.Dispose();
            }
            _instantiatedCustomers.Clear();
            _ordersIndexes.Clear();
            
            SetupInitialOrders();

        }
    }
}