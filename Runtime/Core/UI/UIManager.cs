//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using EasyGameFramework.Core.ObjectPool;
using EasyGameFramework.Core.Resource;

namespace EasyGameFramework.Core.UI
{
    /// <summary>
    /// 界面管理器。
    /// </summary>
    internal sealed partial class UIManager : GameFrameworkModule, IUIManager
    {
        private readonly Dictionary<string, UIGroup> _uIGroups;
        private readonly Dictionary<int, AssetAddress> _uIFormsBeingLoaded;
        private readonly HashSet<int> _uIFormsToReleaseOnLoad;
        private readonly Queue<IUIForm> _recycleQueue;
        private readonly LoadAssetCallbacks _loadAssetCallbacks;
        private IObjectPoolManager _objectPoolManager;
        private IResourceManager _resourceManager;
        private IObjectPool<UIFormInstanceObject> _instancePool;
        private IUIFormHelper _uIFormHelper;
        private int _serial;
        private bool _isShutdown;
        private EventHandler<OpenUIFormSuccessEventArgs> _openUIFormSuccessEventHandler;
        private EventHandler<OpenUIFormFailureEventArgs> _openUIFormFailureEventHandler;
        private EventHandler<CloseUIFormCompleteEventArgs> _closeUIFormCompleteEventHandler;

        /// <summary>
        /// 初始化界面管理器的新实例。
        /// </summary>
        public UIManager()
        {
            _uIGroups = new Dictionary<string, UIGroup>(StringComparer.Ordinal);
            _uIFormsBeingLoaded = new Dictionary<int, AssetAddress>();
            _uIFormsToReleaseOnLoad = new HashSet<int>();
            _recycleQueue = new Queue<IUIForm>();
            _loadAssetCallbacks = new LoadAssetCallbacks(LoadAssetSuccessCallback, LoadAssetFailureCallback);
            _objectPoolManager = null;
            _resourceManager = null;
            _instancePool = null;
            _uIFormHelper = null;
            _serial = 0;
            _isShutdown = false;
            _openUIFormSuccessEventHandler = null;
            _openUIFormFailureEventHandler = null;
            _closeUIFormCompleteEventHandler = null;
        }

        /// <summary>
        /// 获取界面组数量。
        /// </summary>
        public int UIGroupCount
        {
            get
            {
                return _uIGroups.Count;
            }
        }

        /// <summary>
        /// 获取或设置界面实例对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public float InstanceAutoReleaseInterval
        {
            get
            {
                return _instancePool.AutoReleaseInterval;
            }
            set
            {
                _instancePool.AutoReleaseInterval = value;
            }
        }

        /// <summary>
        /// 获取或设置界面实例对象池的容量。
        /// </summary>
        public int InstanceCapacity
        {
            get
            {
                return _instancePool.Capacity;
            }
            set
            {
                _instancePool.Capacity = value;
            }
        }

        /// <summary>
        /// 获取或设置界面实例对象池对象过期秒数。
        /// </summary>
        public float InstanceExpireTime
        {
            get
            {
                return _instancePool.ExpireTime;
            }
            set
            {
                _instancePool.ExpireTime = value;
            }
        }

        /// <summary>
        /// 获取或设置界面实例对象池的优先级。
        /// </summary>
        public int InstancePriority
        {
            get
            {
                return _instancePool.Priority;
            }
            set
            {
                _instancePool.Priority = value;
            }
        }

        /// <summary>
        /// 打开界面成功事件。
        /// </summary>
        public event EventHandler<OpenUIFormSuccessEventArgs> OpenUIFormSuccess
        {
            add
            {
                _openUIFormSuccessEventHandler += value;
            }
            remove
            {
                _openUIFormSuccessEventHandler -= value;
            }
        }

        /// <summary>
        /// 打开界面失败事件。
        /// </summary>
        public event EventHandler<OpenUIFormFailureEventArgs> OpenUIFormFailure
        {
            add
            {
                _openUIFormFailureEventHandler += value;
            }
            remove
            {
                _openUIFormFailureEventHandler -= value;
            }
        }

        /// <summary>
        /// 关闭界面完成事件。
        /// </summary>
        public event EventHandler<CloseUIFormCompleteEventArgs> CloseUIFormComplete
        {
            add
            {
                _closeUIFormCompleteEventHandler += value;
            }
            remove
            {
                _closeUIFormCompleteEventHandler -= value;
            }
        }

        /// <summary>
        /// 界面管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            while (_recycleQueue.Count > 0)
            {
                IUIForm uiForm = _recycleQueue.Dequeue();
                uiForm.OnRecycle();
                _instancePool.Unspawn(uiForm.Handle);
            }

            foreach (KeyValuePair<string, UIGroup> uiGroup in _uIGroups)
            {
                uiGroup.Value.Update(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 关闭并清理界面管理器。
        /// </summary>
        internal override void Shutdown()
        {
            _isShutdown = true;
            CloseAllLoadedUIForms();
            _uIGroups.Clear();
            _uIFormsBeingLoaded.Clear();
            _uIFormsToReleaseOnLoad.Clear();
            _recycleQueue.Clear();
        }

        /// <summary>
        /// 设置对象池管理器。
        /// </summary>
        /// <param name="objectPoolManager">对象池管理器。</param>
        public void SetObjectPoolManager(IObjectPoolManager objectPoolManager)
        {
            if (objectPoolManager == null)
            {
                throw new GameFrameworkException("Object pool manager is invalid.");
            }

            _objectPoolManager = objectPoolManager;
            _instancePool = _objectPoolManager.CreateSingleSpawnObjectPool<UIFormInstanceObject>("UI Instance Pool");
        }

        /// <summary>
        /// 设置资源管理器。
        /// </summary>
        /// <param name="resourceManager">资源管理器。</param>
        public void SetResourceManager(IResourceManager resourceManager)
        {
            if (resourceManager == null)
            {
                throw new GameFrameworkException("Resource manager is invalid.");
            }

            _resourceManager = resourceManager;
        }

        /// <summary>
        /// 设置界面辅助器。
        /// </summary>
        /// <param name="uiFormHelper">界面辅助器。</param>
        public void SetUIFormHelper(IUIFormHelper uiFormHelper)
        {
            if (uiFormHelper == null)
            {
                throw new GameFrameworkException("UI form helper is invalid.");
            }

            _uIFormHelper = uiFormHelper;
        }

        /// <summary>
        /// 是否存在界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>是否存在界面组。</returns>
        public bool HasUIGroup(string uiGroupName)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new GameFrameworkException("UI group name is invalid.");
            }

            return _uIGroups.ContainsKey(uiGroupName);
        }

        /// <summary>
        /// 获取界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>要获取的界面组。</returns>
        public IUIGroup GetUIGroup(string uiGroupName)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new GameFrameworkException("UI group name is invalid.");
            }

            UIGroup uiGroup = null;
            if (_uIGroups.TryGetValue(uiGroupName, out uiGroup))
            {
                return uiGroup;
            }

            return null;
        }

        /// <summary>
        /// 获取所有界面组。
        /// </summary>
        /// <returns>所有界面组。</returns>
        public IUIGroup[] GetAllUIGroups()
        {
            int index = 0;
            IUIGroup[] results = new IUIGroup[_uIGroups.Count];
            foreach (KeyValuePair<string, UIGroup> uiGroup in _uIGroups)
            {
                results[index++] = uiGroup.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有界面组。
        /// </summary>
        /// <param name="results">所有界面组。</param>
        public void GetAllUIGroups(List<IUIGroup> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, UIGroup> uiGroup in _uIGroups)
            {
                results.Add(uiGroup.Value);
            }
        }

        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="uiGroupHelper">界面组辅助器。</param>
        /// <returns>是否增加界面组成功。</returns>
        public bool AddUIGroup(string uiGroupName, IUIGroupHelper uiGroupHelper)
        {
            return AddUIGroup(uiGroupName, 0, uiGroupHelper);
        }

        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="uiGroupDepth">界面组深度。</param>
        /// <param name="uiGroupHelper">界面组辅助器。</param>
        /// <returns>是否增加界面组成功。</returns>
        public bool AddUIGroup(string uiGroupName, int uiGroupDepth, IUIGroupHelper uiGroupHelper)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new GameFrameworkException("UI group name is invalid.");
            }

            if (uiGroupHelper == null)
            {
                throw new GameFrameworkException("UI group helper is invalid.");
            }

            if (HasUIGroup(uiGroupName))
            {
                return false;
            }

            _uIGroups.Add(uiGroupName, new UIGroup(uiGroupName, uiGroupDepth, uiGroupHelper));

            return true;
        }

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIForm(int serialId)
        {
            foreach (KeyValuePair<string, UIGroup> uiGroup in _uIGroups)
            {
                if (uiGroup.Value.HasUIForm(serialId))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="uiFormAssetAddress">界面资源地址。</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIForm(AssetAddress uiFormAssetAddress)
        {
            if (!uiFormAssetAddress.IsValid())
            {
                throw new GameFrameworkException("UI form asset address is invalid.");
            }

            foreach (KeyValuePair<string, UIGroup> uiGroup in _uIGroups)
            {
                if (uiGroup.Value.HasUIForm(uiFormAssetAddress))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm GetUIForm(int serialId)
        {
            foreach (KeyValuePair<string, UIGroup> uiGroup in _uIGroups)
            {
                IUIForm uiForm = uiGroup.Value.GetUIForm(serialId);
                if (uiForm != null)
                {
                    return uiForm;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetAddress">界面资源地址。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm GetUIForm(AssetAddress uiFormAssetAddress)
        {
            if (!uiFormAssetAddress.IsValid())
            {
                throw new GameFrameworkException("UI form asset address is invalid.");
            }

            foreach (KeyValuePair<string, UIGroup> uiGroup in _uIGroups)
            {
                IUIForm uiForm = uiGroup.Value.GetUIForm(uiFormAssetAddress);
                if (uiForm != null)
                {
                    return uiForm;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetAddress">界面资源地址。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm[] GetUIForms(AssetAddress uiFormAssetAddress)
        {
            if (!uiFormAssetAddress.IsValid())
            {
                throw new GameFrameworkException("UI form asset address is invalid.");
            }

            List<IUIForm> results = new List<IUIForm>();
            foreach (KeyValuePair<string, UIGroup> uiGroup in _uIGroups)
            {
                results.AddRange(uiGroup.Value.GetUIForms(uiFormAssetAddress));
            }

            return results.ToArray();
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetAddress">界面资源地址。</param>
        /// <param name="results">要获取的界面。</param>
        public void GetUIForms(AssetAddress uiFormAssetAddress, List<IUIForm> results)
        {
            if (!uiFormAssetAddress.IsValid())
            {
                throw new GameFrameworkException("UI form asset address is invalid.");
            }

            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, UIGroup> uiGroup in _uIGroups)
            {
                uiGroup.Value.InternalGetUIForms(uiFormAssetAddress, results);
            }
        }

        /// <summary>
        /// 获取所有已加载的界面。
        /// </summary>
        /// <returns>所有已加载的界面。</returns>
        public IUIForm[] GetAllLoadedUIForms()
        {
            List<IUIForm> results = new List<IUIForm>();
            foreach (KeyValuePair<string, UIGroup> uiGroup in _uIGroups)
            {
                results.AddRange(uiGroup.Value.GetAllUIForms());
            }

            return results.ToArray();
        }

        /// <summary>
        /// 获取所有已加载的界面。
        /// </summary>
        /// <param name="results">所有已加载的界面。</param>
        public void GetAllLoadedUIForms(List<IUIForm> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, UIGroup> uiGroup in _uIGroups)
            {
                uiGroup.Value.InternalGetAllUIForms(results);
            }
        }

        /// <summary>
        /// 获取所有正在加载界面的序列编号。
        /// </summary>
        /// <returns>所有正在加载界面的序列编号。</returns>
        public int[] GetAllLoadingUIFormSerialIds()
        {
            int index = 0;
            int[] results = new int[_uIFormsBeingLoaded.Count];
            foreach (KeyValuePair<int, AssetAddress> uiFormBeingLoaded in _uIFormsBeingLoaded)
            {
                results[index++] = uiFormBeingLoaded.Key;
            }

            return results;
        }

        /// <summary>
        /// 获取所有正在加载界面的序列编号。
        /// </summary>
        /// <param name="results">所有正在加载界面的序列编号。</param>
        public void GetAllLoadingUIFormSerialIds(List<int> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<int, AssetAddress> uiFormBeingLoaded in _uIFormsBeingLoaded)
            {
                results.Add(uiFormBeingLoaded.Key);
            }
        }

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIForm(int serialId)
        {
            return _uIFormsBeingLoaded.ContainsKey(serialId);
        }

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="uiFormAssetAddress">界面资源地址。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIForm(AssetAddress uiFormAssetAddress)
        {
            if (!uiFormAssetAddress.IsValid())
            {
                throw new GameFrameworkException("UI form asset address is invalid.");
            }

            return _uIFormsBeingLoaded.ContainsValue(uiFormAssetAddress);
        }

        /// <summary>
        /// 是否是合法的界面。
        /// </summary>
        /// <param name="uiForm">界面。</param>
        /// <returns>界面是否合法。</returns>
        public bool IsValidUIForm(IUIForm uiForm)
        {
            if (uiForm == null)
            {
                return false;
            }

            return HasUIForm(uiForm.SerialId);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetAddress">界面资源地址。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="customPriority">加载界面资源的优先级。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(AssetAddress uiFormAssetAddress, string uiGroupName, int? customPriority, bool pauseCoveredUIForm, object userData)
        {
            if (_resourceManager == null)
            {
                throw new GameFrameworkException("You must set resource manager first.");
            }

            if (_uIFormHelper == null)
            {
                throw new GameFrameworkException("You must set UI form helper first.");
            }

            if (!uiFormAssetAddress.IsValid())
            {
                throw new GameFrameworkException("UI form asset address is invalid.");
            }

            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new GameFrameworkException("UI group name is invalid.");
            }

            UIGroup uiGroup = (UIGroup)GetUIGroup(uiGroupName);
            if (uiGroup == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("UI group '{0}' is not exist.", uiGroupName));
            }

            int serialId = ++_serial;
            UIFormInstanceObject uiFormInstanceObject = _instancePool.Spawn(uiFormAssetAddress.Location);
            if (uiFormInstanceObject == null)
            {
                _uIFormsBeingLoaded.Add(serialId, uiFormAssetAddress);
                _resourceManager.LoadAsset(uiFormAssetAddress, _loadAssetCallbacks, null, customPriority, OpenUIFormInfo.Create(serialId, uiGroup, pauseCoveredUIForm, userData));
            }
            else
            {
                InternalOpenUIForm(serialId, uiFormAssetAddress, uiGroup, uiFormInstanceObject.Target, pauseCoveredUIForm, false, 0f, userData);
            }

            return serialId;
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseUIForm(int serialId, object userData)
        {
            if (IsLoadingUIForm(serialId))
            {
                _uIFormsToReleaseOnLoad.Add(serialId);
                _uIFormsBeingLoaded.Remove(serialId);
                return;
            }

            IUIForm uiForm = GetUIForm(serialId);
            if (uiForm == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find UI form '{0}'.", serialId));
            }

            CloseUIForm(uiForm, userData);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseUIForm(IUIForm uiForm, object userData)
        {
            if (uiForm == null)
            {
                throw new GameFrameworkException("UI form is invalid.");
            }

            UIGroup uiGroup = (UIGroup)uiForm.UIGroup;
            if (uiGroup == null)
            {
                throw new GameFrameworkException("UI group is invalid.");
            }

            uiGroup.RemoveUIForm(uiForm);
            uiForm.OnClose(_isShutdown, userData);
            uiGroup.Refresh();

            if (_closeUIFormCompleteEventHandler != null)
            {
                CloseUIFormCompleteEventArgs closeUIFormCompleteEventArgs = CloseUIFormCompleteEventArgs.Create(uiForm.SerialId, uiForm.UIFormAssetAddress, uiGroup, userData);
                _closeUIFormCompleteEventHandler(this, closeUIFormCompleteEventArgs);
                ReferencePool.Release(closeUIFormCompleteEventArgs);
            }

            _recycleQueue.Enqueue(uiForm);
        }

        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        public void CloseAllLoadedUIForms()
        {
            CloseAllLoadedUIForms(null);
        }

        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseAllLoadedUIForms(object userData)
        {
            IUIForm[] uiForms = GetAllLoadedUIForms();
            foreach (IUIForm uiForm in uiForms)
            {
                if (!HasUIForm(uiForm.SerialId))
                {
                    continue;
                }

                CloseUIForm(uiForm, userData);
            }
        }

        /// <summary>
        /// 关闭所有正在加载的界面。
        /// </summary>
        public void CloseAllLoadingUIForms()
        {
            foreach (KeyValuePair<int, AssetAddress> uiFormBeingLoaded in _uIFormsBeingLoaded)
            {
                _uIFormsToReleaseOnLoad.Add(uiFormBeingLoaded.Key);
            }

            _uIFormsBeingLoaded.Clear();
        }

        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        public void RefocusUIForm(IUIForm uiForm)
        {
            RefocusUIForm(uiForm, null);
        }

        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void RefocusUIForm(IUIForm uiForm, object userData)
        {
            if (uiForm == null)
            {
                throw new GameFrameworkException("UI form is invalid.");
            }

            UIGroup uiGroup = (UIGroup)uiForm.UIGroup;
            if (uiGroup == null)
            {
                throw new GameFrameworkException("UI group is invalid.");
            }

            uiGroup.RefocusUIForm(uiForm, userData);
            uiGroup.Refresh();
            uiForm.OnRefocus(userData);
        }

        /// <summary>
        /// 设置界面实例是否被加锁。
        /// </summary>
        /// <param name="uiFormInstance">要设置是否被加锁的界面实例。</param>
        /// <param name="locked">界面实例是否被加锁。</param>
        public void SetUIFormInstanceLocked(object uiFormInstance, bool locked)
        {
            if (uiFormInstance == null)
            {
                throw new GameFrameworkException("UI form instance is invalid.");
            }

            _instancePool.SetLocked(uiFormInstance, locked);
        }

        /// <summary>
        /// 设置界面实例的优先级。
        /// </summary>
        /// <param name="uiFormInstance">要设置优先级的界面实例。</param>
        /// <param name="priority">界面实例优先级。</param>
        public void SetUIFormInstancePriority(object uiFormInstance, int priority)
        {
            if (uiFormInstance == null)
            {
                throw new GameFrameworkException("UI form instance is invalid.");
            }

            _instancePool.SetPriority(uiFormInstance, priority);
        }

        private void InternalOpenUIForm(int serialId, AssetAddress uiFormAssetAddress, UIGroup uiGroup, object uiFormInstance, bool pauseCoveredUIForm, bool isNewInstance, float duration, object userData)
        {
            try
            {
                IUIForm uiForm = _uIFormHelper.CreateUIForm(uiFormInstance, uiGroup, userData);
                if (uiForm == null)
                {
                    throw new GameFrameworkException("Can not create UI form in UI form helper.");
                }

                uiForm.OnInit(serialId, uiFormAssetAddress, uiGroup, pauseCoveredUIForm, isNewInstance, userData);
                uiGroup.AddUIForm(uiForm);
                uiForm.OnOpen(userData);
                uiGroup.Refresh();

                if (_openUIFormSuccessEventHandler != null)
                {
                    OpenUIFormSuccessEventArgs openUIFormSuccessEventArgs = OpenUIFormSuccessEventArgs.Create(uiForm, duration, userData);
                    _openUIFormSuccessEventHandler(this, openUIFormSuccessEventArgs);
                    ReferencePool.Release(openUIFormSuccessEventArgs);
                }
            }
            catch (Exception exception)
            {
                if (_openUIFormFailureEventHandler != null)
                {
                    OpenUIFormFailureEventArgs openUIFormFailureEventArgs = OpenUIFormFailureEventArgs.Create(serialId, uiFormAssetAddress, uiGroup.Name, pauseCoveredUIForm, exception.ToString(), userData);
                    _openUIFormFailureEventHandler(this, openUIFormFailureEventArgs);
                    ReferencePool.Release(openUIFormFailureEventArgs);
                    return;
                }

                throw;
            }
        }

        private void LoadAssetSuccessCallback(AssetAddress assetAddress, object uiFormAsset, float duration, object userData)
        {
            OpenUIFormInfo openUIFormInfo = (OpenUIFormInfo)userData;
            if (openUIFormInfo == null)
            {
                throw new GameFrameworkException("Open UI form info is invalid.");
            }

            if (_uIFormsToReleaseOnLoad.Contains(openUIFormInfo.SerialId))
            {
                _uIFormsToReleaseOnLoad.Remove(openUIFormInfo.SerialId);
                ReferencePool.Release(openUIFormInfo);
                _uIFormHelper.ReleaseUIForm(uiFormAsset, null);
                return;
            }

            _uIFormsBeingLoaded.Remove(openUIFormInfo.SerialId);
            UIFormInstanceObject uiFormInstanceObject = UIFormInstanceObject.Create(assetAddress.Location, uiFormAsset, _uIFormHelper.InstantiateUIForm(uiFormAsset), _uIFormHelper);
            _instancePool.Register(uiFormInstanceObject, true);

            InternalOpenUIForm(openUIFormInfo.SerialId, assetAddress, openUIFormInfo.UIGroup, uiFormInstanceObject.Target, openUIFormInfo.PauseCoveredUIForm, true, duration, openUIFormInfo.UserData);
            ReferencePool.Release(openUIFormInfo);
        }

        private void LoadAssetFailureCallback(AssetAddress assetAddress, LoadResourceStatus status, string errorMessage, object userData)
        {
            OpenUIFormInfo openUIFormInfo = (OpenUIFormInfo)userData;
            if (openUIFormInfo == null)
            {
                throw new GameFrameworkException("Open UI form info is invalid.");
            }

            if (_uIFormsToReleaseOnLoad.Contains(openUIFormInfo.SerialId))
            {
                _uIFormsToReleaseOnLoad.Remove(openUIFormInfo.SerialId);
                return;
            }

            _uIFormsBeingLoaded.Remove(openUIFormInfo.SerialId);
            string appendErrorMessage = Utility.Text.Format("Load UI form failure, asset name '{0}', status '{1}', error message '{2}'.", assetAddress, status, errorMessage);
            if (_openUIFormFailureEventHandler != null)
            {
                OpenUIFormFailureEventArgs openUIFormFailureEventArgs = OpenUIFormFailureEventArgs.Create(openUIFormInfo.SerialId, assetAddress, openUIFormInfo.UIGroup.Name, openUIFormInfo.PauseCoveredUIForm, appendErrorMessage, openUIFormInfo.UserData);
                _openUIFormFailureEventHandler(this, openUIFormFailureEventArgs);
                ReferencePool.Release(openUIFormFailureEventArgs);
                return;
            }

            throw new GameFrameworkException(appendErrorMessage);
        }
    }
}
