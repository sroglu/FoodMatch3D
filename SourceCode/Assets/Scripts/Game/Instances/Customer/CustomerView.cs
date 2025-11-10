using Game.DataStores;
using mehmetsrl.MVC.core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerView : View<CustomerModel>
{
    /*private Text _customerNameText;*/
    [SerializeField] private Image _customerImage;

    [SerializeField] private TMP_Text _customerOrderText;
    [SerializeField] private Image _customerOrderImage;

    public override void UpdateView()
    {
        //_customerNameText.text = Model.CurrentData.DisplayName;
        _customerImage.sprite = Model.CurrentData.Sprite;

        _customerOrderText.text = $"x {Model.Quantity}";
        if (!GameDataStore.Instance.GameData.TryGetPuzzleObjectViewData(Model.OrderId, out var orderViewData))
        {
            Debug.LogError($"Ordered Puzzle Object View Data not found for OrderId: {Model.OrderId}");
            return;
        }

        _customerOrderImage.sprite = orderViewData.Sprite;
    }
}