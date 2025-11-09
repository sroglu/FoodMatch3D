using System;
using System.Collections.Generic;

namespace mehmetsrl.MVC.core
{
    public interface IModel : IDisposable
    {
        uint GetInstanceId(int subModelIndex = 0);
    }

    public enum ModelType
    {
        Single,
        Array
    }
    public abstract class ModelBase : IModel
    {
        public uint InstanceId => _instanceId;
        public bool Initiated => _instanceId > 0;
        public ModelType ModelType => _modelType;
        
        private uint _instanceId = 0;
        private ModelType _modelType = ModelType.Array;

        private static readonly Dictionary<uint, IModel> _modelDictionary = new();

        protected uint RegisterNewModel()
        {
            _instanceId = (uint)_modelDictionary.Count + 1;
            _modelDictionary.Add(_instanceId, this);
            _modelType = ModelType.Single;
            return _instanceId;
        }

        protected static IModel GetModelByInstanceId(uint instanceId)
        {
            return _modelDictionary[instanceId];
        }

        public static void ResetDictionary()
        {
            _modelDictionary.Clear();
        }

        public virtual void Dispose()
        {
            UnregisterModel();
        }
        void UnregisterModel()
        {
            if (ModelType == ModelType.Single)
            {
                _modelDictionary.Remove(_instanceId);
            }
        }

        public abstract uint GetInstanceId(int subModelIndex);
    }

    [System.Serializable]
    public abstract class Model<T> : ModelBase where T : ICloneable
    {
        public override uint GetInstanceId(int subModelIndex)
        {
            if (subModels != null)
                return subModels[subModelIndex].InstanceId;
            return InstanceId;
        }

        public new static Model<T> GetModelByInstanceId(uint instanceId)
        {
            return ModelBase.GetModelByInstanceId(instanceId) as Model<T>;
        }

        protected T DescriptionData { get; private set; }
        protected T[] DescriptionDataArr { get; private set; }
        public T CurrentData { get; protected set; }
        public T[] CurrentDataArr { get; protected set; }
        public Model<T>[] SubModels { get { return subModels; } }
        private Model<T>[] subModels;

        public Model(T data)
        {
            DescriptionData = data;
            UpdateCurrentData();
            RegisterNewModel();
        }

        public Model(T[] dataArr)
        {
            DescriptionDataArr = dataArr;
            UpdateCurrentData();
            CreateSubModels(out subModels);
        }

        #region Operations
        public void Update(T data)
        {
            CurrentData = data;
            UpdateDescriptionData();
        }
        public void Update(T[] data)
        {
            CurrentDataArr = data;
            UpdateDescriptionData();
        }
        public void UpdateCurrentData()
        {
            if (DescriptionData != null)
                UpdateCurrentData(DescriptionData);

            if (DescriptionDataArr != null)
                UpdateCurrentData(DescriptionDataArr);
        }
        void UpdateCurrentData(T data)
        {
            CurrentData = (T)data.Clone();
        }
        void UpdateCurrentData(T[] dataArr)
        {
            if(CurrentDataArr == null || CurrentDataArr.Length != dataArr.Length)
                CurrentDataArr = new T[dataArr.Length];
            Array.Copy(dataArr, CurrentDataArr, dataArr.Length);
        }

        public void UpdateDescriptionData()
        {
            if (CurrentData != null)
                UpdateDescriptionData(CurrentData);

            if (CurrentDataArr != null)
                UpdateDescriptionData(CurrentDataArr);
        }
        void UpdateDescriptionData(T data)
        {
            DescriptionData = (T)data.Clone();
        }
        void UpdateDescriptionData(T[] dataArr)
        {
            Array.Copy(dataArr, DescriptionDataArr, dataArr.Length);
        }
        #endregion


        protected virtual void CreateSubModels(out Model<T>[] subModels) { subModels = null; }
    }
}