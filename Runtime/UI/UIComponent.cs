//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core;
using EasyGameFramework.Core.ObjectPool;
using EasyGameFramework.Core.Resource;
using EasyGameFramework.Core.UI;
using System.Collections.Generic;
using UnityEngine;

namespace EasyGameFramework
{
    /// <summary>
    /// 界面组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/UI")]
    public sealed partial class UIComponent : GameFrameworkComponent
    {
        private IUIManager _uIManager = null;
        private EventComponent _eventComponent = null;
        private IResourceManager _resourceManager = null;

        private readonly List<IUIForm> _internalUIFormResults = new List<IUIForm>();

        [SerializeField]
        private bool _enableOpenUIFormSuccessEvent = true;

        [SerializeField]
        private bool _enableOpenUIFormFailureEvent = true;

        [SerializeField]
        private bool _enableCloseUIFormCompleteEvent = true;

        [SerializeField]
        private float _instanceAutoReleaseInterval = 60f;

        [SerializeField]
        private int _instanceCapacity = 16;

        [SerializeField]
        private float _instanceExpireTime = 60f;

        [SerializeField]
        private int _instancePriority = 0;

        [SerializeField]
        private Transform _instanceRoot = null;

        [SerializeField]
        private string _uIFormHelperTypeName = "UnityGameFramework.Runtime.DefaultUIFormHelper";

        [SerializeField]
        private UIFormHelperBase _customUIFormHelper = null;

        [SerializeField]
        private string _uIGroupHelperTypeName = "UnityGameFramework.Runtime.DefaultUIGroupHelper";

        [SerializeField]
        private UIGroupHelperBase _customUIGroupHelper = null;

        [SerializeField]
        private UIGroup[] _uIGroups = null;

        /// <summary>
        /// 获取界面组数量。
        /// </summary>
        public int UIGroupCount
        {
            get
            {
                return _uIManager.UIGroupCount;
            }
        }

        /// <summary>
        /// 获取或设置界面实例对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public float InstanceAutoReleaseInterval
        {
            get
            {
                return _uIManager.InstanceAutoReleaseInterval;
            }
            set
            {
                _uIManager.InstanceAutoReleaseInterval = _instanceAutoReleaseInterval = value;
            }
        }

        /// <summary>
        /// 获取或设置界面实例对象池的容量。
        /// </summary>
        public int InstanceCapacity
        {
            get
            {
                return _uIManager.InstanceCapacity;
            }
            set
            {
                _uIManager.InstanceCapacity = _instanceCapacity = value;
            }
        }

        /// <summary>
        /// 获取或设置界面实例对象池对象过期秒数。
        /// </summary>
        public float InstanceExpireTime
        {
            get
            {
                return _uIManager.InstanceExpireTime;
            }
            set
            {
                _uIManager.InstanceExpireTime = _instanceExpireTime = value;
            }
        }

        /// <summary>
        /// 获取或设置界面实例对象池的优先级。
        /// </summary>
        public int InstancePriority
        {
            get
            {
                return _uIManager.InstancePriority;
            }
            set
            {
                _uIManager.InstancePriority = _instancePriority = value;
            }
        }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _uIManager = GameFrameworkEntry.GetModule<IUIManager>();
            if (_uIManager == null)
            {
                Log.Fatal("UI manager is invalid.");
                return;
            }

            if (_enableOpenUIFormSuccessEvent)
            {
                _uIManager.OpenUIFormSuccess += OnOpenUIFormSuccess;
            }

            _uIManager.OpenUIFormFailure += OnOpenUIFormFailure;

            if (_enableCloseUIFormCompleteEvent)
            {
                _uIManager.CloseUIFormComplete += OnCloseUIFormComplete;
            }
        }

        private void Start()
        {
            BaseComponent baseComponent = GameEntry.GetComponent<BaseComponent>();
            if (baseComponent == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            _eventComponent = GameEntry.GetComponent<EventComponent>();
            if (_eventComponent == null)
            {
                Log.Fatal("Event component is invalid.");
                return;
            }

            _resourceManager = GameFrameworkEntry.GetModule<IResourceManager>();
            _uIManager.SetResourceManager(_resourceManager);

            _uIManager.SetObjectPoolManager(GameFrameworkEntry.GetModule<IObjectPoolManager>());
            _uIManager.InstanceAutoReleaseInterval = _instanceAutoReleaseInterval;
            _uIManager.InstanceCapacity = _instanceCapacity;
            _uIManager.InstanceExpireTime = _instanceExpireTime;
            _uIManager.InstancePriority = _instancePriority;

            UIFormHelperBase uiFormHelper = Helper.CreateHelper(_uIFormHelperTypeName, _customUIFormHelper);
            if (uiFormHelper == null)
            {
                Log.Error("Can not create UI form helper.");
                return;
            }

            uiFormHelper.name = "UI Form Helper";
            Transform transform = uiFormHelper.transform;
            transform.SetParent(this.transform);
            transform.localScale = Vector3.one;

            _uIManager.SetUIFormHelper(uiFormHelper);

            if (_instanceRoot == null)
            {
                _instanceRoot = new GameObject("UI Form Instances").transform;
                _instanceRoot.SetParent(gameObject.transform);
                _instanceRoot.localScale = Vector3.one;
            }

            _instanceRoot.gameObject.layer = LayerMask.NameToLayer("UI");

            for (int i = 0; i < _uIGroups.Length; i++)
            {
                if (!AddUIGroup(_uIGroups[i].Name, _uIGroups[i].Depth))
                {
                    Log.Warning("Add UI group '{0}' failure.", _uIGroups[i].Name);
                    continue;
                }
            }
        }

        /// <summary>
        /// 是否存在界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>是否存在界面组。</returns>
        public bool HasUIGroup(string uiGroupName)
        {
            return _uIManager.HasUIGroup(uiGroupName);
        }

        /// <summary>
        /// 获取界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>要获取的界面组。</returns>
        public IUIGroup GetUIGroup(string uiGroupName)
        {
            return _uIManager.GetUIGroup(uiGroupName);
        }

        /// <summary>
        /// 获取所有界面组。
        /// </summary>
        /// <returns>所有界面组。</returns>
        public IUIGroup[] GetAllUIGroups()
        {
            return _uIManager.GetAllUIGroups();
        }

        /// <summary>
        /// 获取所有界面组。
        /// </summary>
        /// <param name="results">所有界面组。</param>
        public void GetAllUIGroups(List<IUIGroup> results)
        {
            _uIManager.GetAllUIGroups(results);
        }

        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>是否增加界面组成功。</returns>
        public bool AddUIGroup(string uiGroupName)
        {
            return AddUIGroup(uiGroupName, 0);
        }

        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="depth">界面组深度。</param>
        /// <returns>是否增加界面组成功。</returns>
        public bool AddUIGroup(string uiGroupName, int depth)
        {
            if (_uIManager.HasUIGroup(uiGroupName))
            {
                return false;
            }

            UIGroupHelperBase uiGroupHelper = Helper.CreateHelper(_uIGroupHelperTypeName, _customUIGroupHelper, UIGroupCount);
            if (uiGroupHelper == null)
            {
                Log.Error("Can not create UI group helper.");
                return false;
            }

            uiGroupHelper.name = Utility.Text.Format("UI Group - {0}", uiGroupName);
            uiGroupHelper.gameObject.layer = LayerMask.NameToLayer("UI");
            RectTransform rectTransform = uiGroupHelper.gameObject.GetOrAddComponent<RectTransform>();
            rectTransform.SetParent(_instanceRoot);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            return _uIManager.AddUIGroup(uiGroupName, depth, uiGroupHelper);
        }

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIForm(int serialId)
        {
            return _uIManager.HasUIForm(serialId);
        }

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="uiFormAssetAddress">界面资源地址。</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIForm(AssetAddress uiFormAssetAddress)
        {
            return _uIManager.HasUIForm(uiFormAssetAddress);
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>要获取的界面。</returns>
        public UIForm GetUIForm(int serialId)
        {
            return (UIForm)_uIManager.GetUIForm(serialId);
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetAddress">界面资源地址。</param>
        /// <returns>要获取的界面。</returns>
        public UIForm GetUIForm(AssetAddress uiFormAssetAddress)
        {
            return (UIForm)_uIManager.GetUIForm(uiFormAssetAddress);
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetAddress">界面资源地址。</param>
        /// <returns>要获取的界面。</returns>
        public UIForm[] GetUIForms(AssetAddress uiFormAssetAddress)
        {
            IUIForm[] uiForms = _uIManager.GetUIForms(uiFormAssetAddress);
            UIForm[] uiFormImpls = new UIForm[uiForms.Length];
            for (int i = 0; i < uiForms.Length; i++)
            {
                uiFormImpls[i] = (UIForm)uiForms[i];
            }

            return uiFormImpls;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetAddress">界面资源地址。</param>
        /// <param name="results">要获取的界面。</param>
        public void GetUIForms(AssetAddress uiFormAssetAddress, List<UIForm> results)
        {
            if (results == null)
            {
                Log.Error("Results is invalid.");
                return;
            }

            results.Clear();
            _uIManager.GetUIForms(uiFormAssetAddress, _internalUIFormResults);
            foreach (IUIForm uiForm in _internalUIFormResults)
            {
                results.Add((UIForm)uiForm);
            }
        }

        /// <summary>
        /// 获取所有已加载的界面。
        /// </summary>
        /// <returns>所有已加载的界面。</returns>
        public UIForm[] GetAllLoadedUIForms()
        {
            IUIForm[] uiForms = _uIManager.GetAllLoadedUIForms();
            UIForm[] uiFormImpls = new UIForm[uiForms.Length];
            for (int i = 0; i < uiForms.Length; i++)
            {
                uiFormImpls[i] = (UIForm)uiForms[i];
            }

            return uiFormImpls;
        }

        /// <summary>
        /// 获取所有已加载的界面。
        /// </summary>
        /// <param name="results">所有已加载的界面。</param>
        public void GetAllLoadedUIForms(List<UIForm> results)
        {
            if (results == null)
            {
                Log.Error("Results is invalid.");
                return;
            }

            results.Clear();
            _uIManager.GetAllLoadedUIForms(_internalUIFormResults);
            foreach (IUIForm uiForm in _internalUIFormResults)
            {
                results.Add((UIForm)uiForm);
            }
        }

        /// <summary>
        /// 获取所有正在加载界面的序列编号。
        /// </summary>
        /// <returns>所有正在加载界面的序列编号。</returns>
        public int[] GetAllLoadingUIFormSerialIds()
        {
            return _uIManager.GetAllLoadingUIFormSerialIds();
        }

        /// <summary>
        /// 获取所有正在加载界面的序列编号。
        /// </summary>
        /// <param name="results">所有正在加载界面的序列编号。</param>
        public void GetAllLoadingUIFormSerialIds(List<int> results)
        {
            _uIManager.GetAllLoadingUIFormSerialIds(results);
        }

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIForm(int serialId)
        {
            return _uIManager.IsLoadingUIForm(serialId);
        }

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="uiFormAssetAddress">界面资源地址。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIForm(AssetAddress uiFormAssetAddress)
        {
            return _uIManager.IsLoadingUIForm(uiFormAssetAddress);
        }

        /// <summary>
        /// 是否是合法的界面。
        /// </summary>
        /// <param name="uiForm">界面。</param>
        /// <returns>界面是否合法。</returns>
        public bool IsValidUIForm(UIForm uiForm)
        {
            return _uIManager.IsValidUIForm(uiForm);
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
        public int OpenUIForm(
            AssetAddress uiFormAssetAddress,
            string uiGroupName,
            int? customPriority = null,
            bool pauseCoveredUIForm = false,
            object userData = null)
        {
            return _uIManager.OpenUIForm(uiFormAssetAddress, uiGroupName, customPriority, pauseCoveredUIForm, userData);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseUIForm(int serialId, object userData = null)
        {
            _uIManager.CloseUIForm(serialId, userData);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseUIForm(UIForm uiForm, object userData = null)
        {
            _uIManager.CloseUIForm(uiForm, userData);
        }

        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        public void CloseAllLoadedUIForms()
        {
            _uIManager.CloseAllLoadedUIForms();
        }

        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseAllLoadedUIForms(object userData)
        {
            _uIManager.CloseAllLoadedUIForms(userData);
        }

        /// <summary>
        /// 关闭所有正在加载的界面。
        /// </summary>
        public void CloseAllLoadingUIForms()
        {
            _uIManager.CloseAllLoadingUIForms();
        }

        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        public void RefocusUIForm(UIForm uiForm)
        {
            _uIManager.RefocusUIForm(uiForm);
        }

        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void RefocusUIForm(UIForm uiForm, object userData)
        {
            _uIManager.RefocusUIForm(uiForm, userData);
        }

        /// <summary>
        /// 设置界面是否被加锁。
        /// </summary>
        /// <param name="uiForm">要设置是否被加锁的界面。</param>
        /// <param name="locked">界面是否被加锁。</param>
        public void SetUIFormInstanceLocked(UIForm uiForm, bool locked)
        {
            if (uiForm == null)
            {
                Log.Warning("UI form is invalid.");
                return;
            }

            _uIManager.SetUIFormInstanceLocked(uiForm.gameObject, locked);
        }

        /// <summary>
        /// 设置界面的优先级。
        /// </summary>
        /// <param name="uiForm">要设置优先级的界面。</param>
        /// <param name="priority">界面优先级。</param>
        public void SetUIFormInstancePriority(UIForm uiForm, int priority)
        {
            if (uiForm == null)
            {
                Log.Warning("UI form is invalid.");
                return;
            }

            _uIManager.SetUIFormInstancePriority(uiForm.gameObject, priority);
        }

        private void OnOpenUIFormSuccess(object sender, EasyGameFramework.Core.UI.OpenUIFormSuccessEventArgs e)
        {
            _eventComponent.Fire(this, OpenUIFormSuccessEventArgs.Create(e));
        }

        private void OnOpenUIFormFailure(object sender, EasyGameFramework.Core.UI.OpenUIFormFailureEventArgs e)
        {
            Log.Warning("Open UI form failure, asset name '{0}', UI group name '{1}', pause covered UI form '{2}', error message '{3}'.", e.UIFormAssetAddress, e.UIGroupName, e.PauseCoveredUIForm, e.ErrorMessage);
            if (_enableOpenUIFormFailureEvent)
            {
                _eventComponent.Fire(this, OpenUIFormFailureEventArgs.Create(e));
            }
        }

        private void OnCloseUIFormComplete(object sender, EasyGameFramework.Core.UI.CloseUIFormCompleteEventArgs e)
        {
            _eventComponent.Fire(this, CloseUIFormCompleteEventArgs.Create(e));
        }
    }
}
