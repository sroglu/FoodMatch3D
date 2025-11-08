using System;

namespace mehmetsrl.DataStore
{
public interface IDataStoreDatabase : IDisposable { }
    public interface IDataStoreDatabase<T> : IDataStoreDatabase where T : IDataStoreDatabase<T>, new()
    {
        protected static bool _initialized;
        
        protected static T _instance;
        
        public static T Instance
        {
            get
            {
                if (_initialized)
                {
                    return _instance;
                }
                #if UNITY_EDITOR
                // In editor mode, we just show database to see its content without initialization
                Initialize();
                if (!_initialized)
                {
                    throw new Exception($" Datastore database {typeof(T)} is not initialized");
                }
                return _instance;
#else
                throw new Exception($" Datastore database {typeof(T)} is not initialized");
#endif
            }
        }

        protected static void Initialize()
        {
            if (_initialized)
            {
                throw new Exception($" Datastore database {typeof(T)} is already initialized");
            }
            
            var type = typeof(T);
            _instance = (T)DataStoreManager.Instance.CreateDatabase(typeof(T));
            _initialized = true;
        }
        
        protected static void Destroy()
        {
            if (!_initialized)
            {
                throw new Exception($" Datastore database {typeof(T)} is not initialized");
            }
            _initialized = false;
            _instance = default;
        }
    }
    [Serializable]
    public abstract class DataStoreClass<T> : IDataStoreDatabase<T> where T : IDataStoreDatabase<T>, new()
    {
        public static T Instance => IDataStoreDatabase<T>.Instance;
        public void Dispose() => IDataStoreDatabase<T>.Destroy(); 
        public static bool IsInitialized => IDataStoreDatabase<T>._initialized;
        
        public static void Initialize()=>IDataStoreDatabase<T>.Initialize();
    }
    [Serializable]
    public abstract record DataStoreRecord<T> : IDataStoreDatabase<T> where T : IDataStoreDatabase<T>, new()
    {
        public static T Instance => IDataStoreDatabase<T>.Instance;
        public void Dispose() => IDataStoreDatabase<T>.Destroy(); 
        public static bool IsInitialized => IDataStoreDatabase<T>._initialized;
        
        public static void Initialize()=>IDataStoreDatabase<T>.Initialize();
    }
}