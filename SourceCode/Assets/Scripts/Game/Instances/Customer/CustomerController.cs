using DG.Tweening;
using mehmetsrl.MVC;
using mehmetsrl.MVC.core;
using UnityEngine;

public class CustomerController : Controller<CustomerView, CustomerModel>
{
    public CustomerController(CustomerModel model, CustomerView view = null) : base(ControllerType.Instance, model, view)
    {
        
    }

    public void SetOrder(uint orderDataOrderId, uint orderDataQuantity)
    {
        Model.SetOrder(orderDataOrderId, orderDataQuantity);
        View.UpdateView();
    }

    public void EndOrder()
    {
        //play pop animation than destroy
        Dispose();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        var popAnimationPosition =  View.RectTransform.position;
        //TODO: Play vfx at popAnimationPosition

    }
}
