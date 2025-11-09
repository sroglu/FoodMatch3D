using System;
using System.Collections.Generic;
using Game.Data;
using mehmetsrl.MVC.core;
using UnityEngine;
using UnityEngine.Pool;

public class PooledObjectBehaviour : MonoBehaviour
{
    public int IdentifierId { get; internal set; }

    protected void Recycle()
    {
        OnRecycle();
        InstanceManager.Instance.ReturnInstance(this);
    }

    protected virtual void OnRecycle() { }
}

public class InstanceManager : MonoBehaviour, IDisposable
{
    //Singleton
    private static InstanceManager _instance;
    public static InstanceManager Instance
    {
        get { return _instance; }
        private set
        {
            if (_instance == null)
                _instance = value;
            else
                Destroy(value);
        }
    }

    private void Awake()
    {
        Instance = this;
    }
    
    private static readonly Dictionary<int, ObjectPool<PooledObjectBehaviour>> _cachedPools = new();
    
    private T CreatePool<T>(Func<PooledObjectBehaviour> createFunc, Action<PooledObjectBehaviour> actionOnGet,
        Action<PooledObjectBehaviour> actionOnRelease, Action<PooledObjectBehaviour> actionOnDestroy,
        int defaultCapacity = 10, int maxSize = 100) where T : ObjectPool<PooledObjectBehaviour>
    {
        var id = typeof(T).GetHashCode();
        if (!_cachedPools.ContainsKey(id))
        {
            var objectPool = new ObjectPool<PooledObjectBehaviour>(createFunc, actionOnGet, actionOnRelease,
                actionOnDestroy, false, defaultCapacity, maxSize);
            _cachedPools.Add(id, objectPool);
        }
        return _cachedPools[id] as T;
    }
    
    private ObjectPool<PooledObjectBehaviour> GetOrCreatePool(PooledObjectBehaviour pooledObject){
        var id = pooledObject.GetType().GetHashCode();
        if (_cachedPools.TryGetValue(id, out var pool))
        {
            return pool;
        }

        pool = new ObjectPool<PooledObjectBehaviour>(() =>
            {
                var instance = Instantiate(pooledObject);
                instance.IdentifierId = id;
                return instance;
            },
            obj => { obj.gameObject.SetActive(true); },
            obj => { obj.gameObject.SetActive(false); },
            obj => { Destroy(obj.gameObject); },
            false, 10, 100);

        _cachedPools.Add(id, pool);
        return pool;
    }
    
    public T SpawnInstance<T>(PooledObjectBehaviour pooledObject)
    {
        var instance = GetOrCreatePool(pooledObject).Get();
        return instance.GetComponent<T>();
    }
    
    public T SpawnWithoutPool<T>(PooledObjectBehaviour pooledObject) where T : PooledObjectBehaviour
    {
        var instance = Instantiate(pooledObject);
        return instance.GetComponent<T>();
    }
    
    
    public void ReturnInstance(PooledObjectBehaviour pooledObjectBehaviour)
    {
        if (_cachedPools.TryGetValue(pooledObjectBehaviour.IdentifierId, out var pool))
        {
            pool.Release(pooledObjectBehaviour);
        }
        else if (pooledObjectBehaviour != null)
        {
            Debug.LogWarning($"No pool exists for pooled object {pooledObjectBehaviour.name}-{pooledObjectBehaviour.IdentifierId}. Destroying object.");
            Destroy(pooledObjectBehaviour.gameObject);
        }
    }



    #region UI Spesific Methods
    
    /// <summary>
    /// Clear all instances under management of this manager.
    /// Some instances might be moved to other transforms.
    /// Those instances should managed by their responsible managers or controllers.
    /// </summary>
    public void ClearAllInstances()
    {
        foreach (var instance in GetComponentsInChildren<ViewBase>())
        {
            instance.DestroyInstance();
        }
    }
    public void Dispose()
    {
        ClearAllInstances();
        
        foreach (var pool in _cachedPools.Values)
        {
            pool.Clear();
        }
        _cachedPools.Clear();
    }
    
    /// <summary>
    /// Builder method for customers
    /// </summary>
    /// <param name="customerViewData">Customer view data</param>
    /// <returns>Customer controller</returns>
    public CustomerController CreateCustomer(CustomerViewData customerViewData)
    {
        var customer = new CustomerController(new CustomerModel(customerViewData));
        customer.View.transform.parent = transform;
        return customer;
    }
    
    
    /*/// <summary>
    /// Builder method for action buttons
    /// </summary>
    /// <param name="gameActionData"></param>
    /// <returns></returns>
    public ActionButtonController CreateActionButton(GameActionData gameActionData)
    {
        var actionButton = new ActionButtonController(new ActionButtonModel(gameActionData));
        actionButton.View.transform.parent = transform;
        return actionButton;
    }*/
    

    #endregion
    
}
