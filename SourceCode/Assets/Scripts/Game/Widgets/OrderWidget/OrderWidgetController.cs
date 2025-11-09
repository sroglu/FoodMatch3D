using System.Collections.Generic;
using Game.Data;
using mehmetsrl.MVC.core;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Widgets.OrderWidget
{
    public class OrderWidgetController : Controller<OrderWidgetView, OrderWidgetModel>
    {
        
        private readonly List<(int,OrderData)> _orders = new();
        public OrderWidgetController(OrderWidgetModel model, OrderWidgetView view = null) : base(ControllerType.Instance, model, view) { }

        protected override void OnCreate()
        {
            for (int i = 0; i < Model.CurrentDataArr.Length; i++)
            {
                _orders.Add((i,Model.CurrentDataArr[i]));
            }

            FilterAndDisplayOrders();
        }
        
        private void UpdateOrders()
        {
            //Update orders and remove completed ones
            for (int i = 0; i < _orders.Count; i++)
            {
                var orderData = _orders[i].Item2;
                if (orderData.IsCompleted)
                {
                    Model.DecrementOrderCount();
                    _orders.RemoveAt(i);
                    i--;
                }
            }

            if (_orders.Count > 0)
            {
                FilterAndDisplayOrders();
            }
            else
            {
                //Complete level
                Debug.Log("All orders completed!");
                GameManager.Instance.CompleteLevel(true);
            }
        }
        
        private void FilterAndDisplayOrders()
        {
            var ordersToBeDisplayed = new (int, OrderData)[math.clamp(_orders.Count, 0, View.OrderLimitAtTheSameTime)];
            for (int i = 0; i < ordersToBeDisplayed.Length; i++)
            {
                ordersToBeDisplayed[i] = _orders[i];
            }
            View.UpdateOrders(ordersToBeDisplayed);
        }
    }
}