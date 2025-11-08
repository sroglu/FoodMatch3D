using System;
using mehmetsrl.MVC.core;
using UnityEngine;

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
    }
}
