using mehmetsrl.MVC.core;

public class CustomerController : Controller<CustomerView, CustomerModel>
{
    public CustomerController(ControllerType controllerType, CustomerModel model, CustomerView view = null) : base(controllerType, model, view)
    {
        
    }
}
