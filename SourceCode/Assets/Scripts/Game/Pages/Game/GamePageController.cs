using System;
using Game.Data;
using Game.DataStores;
using Game.Instances.PuzzleInstances;
using Game.Widgets.MatchWidget;
using Game.Widgets.OrderWidget;
using mehmetsrl.MVC.core;
using UnityEngine;

/// <summary>
/// Game page controller.
/// It consists of 2 mvc components called widgets.
/// </summary>
public class GamePageController : Controller<GamePageView, GamePageModel>
{
    private OrderWidgetController _orderWidgetController;
    private MatchBoardController _matchBoardController;
    
    private static readonly int _excludeViewMask = ~LayerMask.GetMask("View");
    
    public GamePageController(GamePageModel model) : base(ControllerType.Page, model)
    {
    }

    protected override void OnCreate()
    {
        var orderCount = 0;
        var orderData = new OrderData[Model.CurrentData.Level.PuzzleObjects.Length];
        for (int i = 0; i < orderData.Length; i++)
        {
            var existingPuzzleObject = Model.CurrentData.Level.PuzzleObjects[i];
            
            if(!existingPuzzleObject.IsOrdered) continue;
            
            //Get random customer type id for this puzzle object without using GameDataStore
            var customerTypeId = GameDataStore.Instance.GameData.GetRandomCustomerTypeId();
            orderData[orderCount] = new OrderData(customerTypeId, existingPuzzleObject.TypeId, existingPuzzleObject.Quantity);
            orderCount++;
        }
        Array.Resize(ref orderData, orderCount);
        
        _orderWidgetController = new OrderWidgetController(new OrderWidgetModel(orderData), View.OrderWidgetView);
        _matchBoardController = new MatchBoardController(View.MatchBoardView.Model, View.MatchBoardView);

    }
    
    public void OnViewEnabled()
    {
        _orderWidgetController.View.Show();
        _matchBoardController.View.Show();
    }
    public void OnViewDisabled()
    {
        _orderWidgetController.View.Hide();
        _matchBoardController.View.Hide();
    }

    private void OnPuzzleObjectClicked(PuzzleObjectInstance puzzleObject)
    {
        _matchBoardController.AddToMatchBoard(puzzleObject);
        
        ApplyTouchEffectToOtherObjectsAround(puzzleObject);
        
    }

    private void ApplyTouchEffectToOtherObjectsAround(PuzzleObjectInstance puzzleObject)
    {
        var center = puzzleObject.transform.position;
        
        // compute object radius from Collider or Renderer bounds, fallback to a small default
        float objectRadius = 0f;
        var objCollider = puzzleObject.GetComponentInChildren<Collider>();
        if (objCollider != null)
        {
            objectRadius = objCollider.bounds.extents.magnitude;
        }
        else
        {
            var renderer = puzzleObject.GetComponentInChildren<Renderer>();
            if (renderer != null)
                objectRadius = renderer.bounds.extents.magnitude;
            else
                objectRadius = 0.5f; // fallback if no bounds available
        }

        // use the object's size as radius, add slight padding
        var radius = Mathf.Max(0.01f, objectRadius * 1.2f);
        const float impulseStrength = 5f;

        var colliders = Physics.OverlapSphere(center, radius);
        foreach (var col in colliders)
        {
            var rb = col.attachedRigidbody;
            if (rb == null) continue;
            if (rb.isKinematic) continue;
            if (rb.gameObject == puzzleObject.gameObject) continue;

            var dir = rb.worldCenterOfMass - center;
            var dist = dir.magnitude;
            if (dist < 0.01f) dir = UnityEngine.Random.onUnitSphere;

            var attenuation = 1f - Mathf.Clamp01(dist / radius);
            var impulse = dir.normalized * impulseStrength * attenuation;

            rb.AddForce(impulse, ForceMode.Impulse);
            rb.AddTorque(UnityEngine.Random.onUnitSphere * (impulseStrength * 0.1f), ForceMode.Impulse);
        }
        
    }


    private void RaycastPuzzleObjects()
    {
        var camera = GameDataStore.Instance.GameCamera;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        Debug.DrawRay(ray.origin, ray.direction, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, camera.farClipPlane, _excludeViewMask))
        {
            Debug.DrawLine(ray.origin, hitInfo.point, Color.blue);
            var puzzleObject = hitInfo.collider.GetComponentInParent<PuzzleObjectInstance>();
            if (puzzleObject != null)
            {
                Debug.Log($"Hit Puzzle Object TypeId: {puzzleObject.TypeId}");
                OnPuzzleObjectClicked(puzzleObject);
            }
        }
    }

    private float _timeSinceLastClick = 0f;
    private const float ClickCooldown = 0.2f;
    public void OnRaycastBlockerClicked()
    {
        if (_timeSinceLastClick + ClickCooldown > Time.realtimeSinceStartup) return;
        RaycastPuzzleObjects();
        _timeSinceLastClick = Time.realtimeSinceStartup;
    }
}