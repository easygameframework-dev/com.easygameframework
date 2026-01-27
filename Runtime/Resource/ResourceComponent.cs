//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core;
using EasyGameFramework.Core.Download;
using EasyGameFramework.Core.FileSystem;
using EasyGameFramework.Core.ObjectPool;
using EasyGameFramework.Core.Resource;
using System;
using System.Collections.Generic;
using UnityEngine;
using PlayMode = EasyGameFramework.Core.Resource.PlayMode;

namespace EasyGameFramework
{
    /// <summary>
    /// 资源组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Resource")]
    public sealed class ResourceComponent : GameFrameworkComponent
    {
        private const int DefaultPriority = 0;

        private IResourceManager _resourceManager = null;
        // private EventComponent _eventComponent = null;

        /// <summary>
        /// 资源系统运行模式。
        /// </summary>
        [SerializeField] private PlayMode _playMode = PlayMode.EditorSimulateMode;

        /// <summary>
        /// 下载文件校验等级。
        /// </summary>
        [SerializeField] private FileVerifyLevel _fileVerifyLevel = FileVerifyLevel.Middle;

        [SerializeField] private ReadWritePathType _readWritePathType = ReadWritePathType.Unspecified;

        /// <summary>
        /// 资源包名称。
        /// </summary>
        [SerializeField] private string _defaultPackageName = "DefaultPackage";

        /// <summary>
        /// 设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）
        /// </summary>
        [SerializeField] private long _milliseconds = 30;

        [SerializeField] private float _assetAutoReleaseInterval = 60f;

        [SerializeField] private int _assetCapacity = 64;

        [SerializeField] private float _assetExpireTime = 60f;

        [SerializeField] private int _assetPriority = 0;

        [SerializeField] private float _minUnloadUnusedAssetsInterval = 60f;

        [SerializeField] private float _maxUnloadUnusedAssetsInterval = 300f;

        [SerializeField] private bool _useSystemUnloadUnusedAssets = true;

        [SerializeField] private int _downloadingMaxNum = 10;

        [SerializeField] private int _failedTryAgain = 3;

        [SerializeField] private LoadResourceAgentHelperBase _loadResourceAgentHelper = null;
        [SerializeField] private ResourceHelperBase _resourceHelper = null;

        private float _lastUnloadUnusedAssetsOperationElapseSeconds = 0f;

        public PlayMode PlayMode => _playMode;
        public FileVerifyLevel FileVerifyLevel => _fileVerifyLevel;

        public float LastUnloadUnusedAssetsOperationElapseSeconds => _lastUnloadUnusedAssetsOperationElapseSeconds;

        public long Milliseconds => _milliseconds;

        public string DefaultPackageName => _defaultPackageName;

        public float MaxUnloadUnusedAssetsInterval => _maxUnloadUnusedAssetsInterval;

        public int DownloadingMaxNum => _downloadingMaxNum;

        public int FailedTryAgain => _failedTryAgain;

        /// <summary>
        /// 获取资源只读路径。
        /// </summary>
        public string ReadOnlyPath => _resourceManager.ReadOnlyPath;

        /// <summary>
        /// 获取资源读写路径。
        /// </summary>
        public string ReadWritePath => _resourceManager.ReadWritePath;

        /// <summary>
        /// 获取或设置资源对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public float AssetAutoReleaseInterval
        {
            get { return _resourceManager.AssetAutoReleaseInterval; }
            set { _resourceManager.AssetAutoReleaseInterval = _assetAutoReleaseInterval = value; }
        }

        /// <summary>
        /// 资源服务器地址。
        /// </summary>
        public string HostServerURL { get; set; }

        public string FallbackHostServerURL { get; set; }

        /// <summary>
        /// 获取或设置资源对象池的容量。
        /// </summary>
        public int AssetCapacity
        {
            get { return _resourceManager.AssetCapacity; }
            set { _resourceManager.AssetCapacity = _assetCapacity = value; }
        }

        /// <summary>
        /// 获取或设置资源对象池对象过期秒数。
        /// </summary>
        public float AssetExpireTime
        {
            get { return _resourceManager.AssetExpireTime; }
            set { _resourceManager.AssetExpireTime = _assetExpireTime = value; }
        }

        /// <summary>
        /// 获取或设置资源对象池的优先级。
        /// </summary>
        public int AssetPriority
        {
            get { return _resourceManager.AssetPriority; }
            set { _resourceManager.AssetPriority = _assetPriority = value; }
        }

        public bool IsInitialized { get; private set; }

        private void Start()
        {
            BaseComponent baseComponent = GameEntry.GetComponent<BaseComponent>();
            if (baseComponent == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            _resourceManager = GameFrameworkEntry.GetModule<IResourceManager>();
            if (_resourceManager == null)
            {
                Log.Fatal("Resource component is invalid.");
                return;
            }

            if (_playMode == PlayMode.EditorSimulateMode)
            {
                Log.Debug(
                    "During this run, Game Framework will use editor resource files, which you should validate first.");
#if !UNITY_EDITOR
                _playMode = PlayMode.OfflinePlayMode;
#endif
            }

            _resourceManager.SetReadOnlyPath(Application.streamingAssetsPath);
            if (_readWritePathType == ReadWritePathType.TemporaryCache)
            {
                _resourceManager.SetReadWritePath(Application.temporaryCachePath);
            }
            else
            {
                if (_readWritePathType == ReadWritePathType.Unspecified)
                {
                    _readWritePathType = ReadWritePathType.PersistentData;
                }

                _resourceManager.SetReadWritePath(Application.persistentDataPath);
            }

            _resourceManager.AddLoadResourceAgentHelper(_loadResourceAgentHelper);
            _resourceManager.SetResourceHelper(_resourceHelper);

            _resourceManager.PlayMode = _playMode;
            _resourceManager.FileVerifyLevel = _fileVerifyLevel;
            _resourceManager.Milliseconds = _milliseconds;
            _resourceManager.AssetAutoReleaseInterval = _assetAutoReleaseInterval;
            _resourceManager.AssetCapacity = _assetCapacity;
            _resourceManager.AssetExpireTime = _assetExpireTime;
            _resourceManager.AssetPriority = _assetPriority;
            IsInitialized = true;
            Log.Debug($"ResourceComponent Run Mode：{_playMode}");
        }

        /// <summary>
        /// 检查资源是否存在。
        /// </summary>
        /// <param name="assetAddress">要检查资源的地址。</param>
        /// <returns>检查资源是否存在的结果。</returns>
        public HasAssetResult HasAsset(AssetAddress assetAddress)
        {
            return _resourceManager.HasAsset(assetAddress);
        }

        /// <summary>
        /// 获取资源信息。
        /// </summary>
        /// <param name="assetAddress">要获取资源信息的地址。</param>
        /// <returns></returns>
        public AssetInfo GetAssetInfo(AssetAddress assetAddress)
        {
            return _resourceManager.GetAssetInfo(assetAddress);
        }

        /// <summary>
        /// 获取资源信息数组。
        /// </summary>
        /// <param name="packageName">资源包名称。</param>
        /// <param name="tags">资源标签数组。</param>
        /// <returns>资源信息数组。</returns>
        public AssetInfo[] GetAssetInfos(string packageName, params string[] tags)
        {
            return _resourceManager.GetAssetInfos(packageName, tags);
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetAddress">要加载资源的地址。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="assetType">要加载资源的类型。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadAsset(
            AssetAddress assetAddress,
            LoadAssetCallbacks loadAssetCallbacks,
            Type assetType = null,
            int? priority = null,
            object userData = null)
        {
            _resourceManager.LoadAsset(assetAddress, loadAssetCallbacks, assetType, priority, userData);
        }

        public void UnloadAsset(object asset)
        {
            _resourceManager.UnloadAsset(asset);
        }

        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetAddress">要加载场景资源的地址。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        /// <param name="priority">加载场景资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadScene(
            AssetAddress sceneAssetAddress,
            LoadSceneCallbacks loadSceneCallbacks,
            int? priority = null,
            object userData = null)
        {
            _resourceManager.LoadScene(sceneAssetAddress, loadSceneCallbacks, priority, userData);
        }

        /// <summary>
        /// 异步卸载场景。
        /// </summary>
        /// <param name="sceneAssetAddress">要卸载场景资源的地址。</param>
        /// <param name="unloadSceneCallbacks">卸载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void UnloadScene(
            AssetAddress sceneAssetAddress,
            UnloadSceneCallbacks unloadSceneCallbacks,
            object userData = null)
        {
            _resourceManager.UnloadScene(sceneAssetAddress, unloadSceneCallbacks, userData);
        }

        public void ClearAllCacheFiles(
            FileClearMode fileClearMode,
            ClearAllCacheFilesCallbacks clearAllCacheFilesCallbacks,
            object userData = null)
        {
            _resourceManager.ClearAllCacheFiles(fileClearMode, clearAllCacheFilesCallbacks, userData);
        }
        
        public void ClearPackageCacheFiles(
            string packageName,
            FileClearMode fileClearMode,
            ClearPackageCacheFilesCallbacks clearPackageCacheFilesCallbacks,
            object userData = null)
        {
            _resourceManager.ClearPackageCacheFiles(packageName, fileClearMode, clearPackageCacheFilesCallbacks, userData);
        }
    }
}
