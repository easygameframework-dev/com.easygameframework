//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core.Download;
using EasyGameFramework.Core.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using EasyGameFramework.Core.ObjectPool;

namespace EasyGameFramework.Core.Resource
{
    /// <summary>
    /// 资源管理器。
    /// </summary>
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        private string _readOnlyPath;
        private string _readWritePath;
        private IResourceHelper _resourceHelper;

        private readonly ResourceLoader _resourceLoader;
        private readonly Dictionary<AssetAddress, AssetInfo> _assetInfosCache;
        private readonly Dictionary<(string packageName, string[] tags), AssetInfo[]> _assetInfosCacheByTags;

        /// <summary>
        /// 初始化资源管理器的新实例。
        /// </summary>
        public ResourceManager()
        {
            _readOnlyPath = null;
            _readWritePath = null;
            _resourceLoader = new ResourceLoader(this);
            _assetInfosCache = new Dictionary<AssetAddress, AssetInfo>();
            _assetInfosCacheByTags = new Dictionary<(string packageName, string[] tags), AssetInfo[]>();
        }

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        internal override int Priority
        {
            get { return 3; }
        }

        public PlayMode PlayMode { get; set; }
        public FileVerifyLevel FileVerifyLevel { get; set; }
        public int DownloadingMaxNum { get; set; }
        public int FailedTryAgain { get; set; }

        /// <summary>
        /// 获取资源只读区路径。
        /// </summary>
        public string ReadOnlyPath
        {
            get { return _readOnlyPath; }
        }

        /// <summary>
        /// 获取资源读写区路径。
        /// </summary>
        public string ReadWritePath
        {
            get { return _readWritePath; }
        }

        public long Milliseconds { get; set; }
        public float AssetAutoReleaseInterval { get; set; }
        public int AssetCapacity { get; set; }
        public float AssetExpireTime { get; set; }
        public int AssetPriority { get; set; }

        /// <summary>
        /// 资源管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            _resourceLoader.Update(elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 关闭并清理资源管理器。
        /// </summary>
        internal override void Shutdown()
        {
            _resourceLoader.Shutdown();
        }

        /// <summary>
        /// 设置资源只读区路径。
        /// </summary>
        /// <param name="readOnlyPath">资源只读区路径。</param>
        public void SetReadOnlyPath(string readOnlyPath)
        {
            if (string.IsNullOrEmpty(readOnlyPath))
            {
                throw new GameFrameworkException("Read-only path is invalid.");
            }

            _readOnlyPath = readOnlyPath;
        }

        /// <summary>
        /// 设置资源读写区路径。
        /// </summary>
        /// <param name="readWritePath">资源读写区路径。</param>
        public void SetReadWritePath(string readWritePath)
        {
            if (string.IsNullOrEmpty(readWritePath))
            {
                throw new GameFrameworkException("Read-write path is invalid.");
            }

            _readWritePath = readWritePath;
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
            _resourceLoader.SetObjectPoolManager(objectPoolManager);
        }

        /// <summary>
        /// 添加加载资源代理辅助器。
        /// </summary>
        /// <param name="loadResourceAgentHelper">要添加的加载资源代理辅助器。</param>
        public void AddLoadResourceAgentHelper(ILoadResourceAgentHelper loadResourceAgentHelper)
        {
            if (string.IsNullOrEmpty(_readOnlyPath))
            {
                throw new GameFrameworkException("Read-only path is invalid.");
            }

            if (string.IsNullOrEmpty(_readWritePath))
            {
                throw new GameFrameworkException("Read-write path is invalid.");
            }

            _resourceLoader.AddLoadResourceAgentHelper(loadResourceAgentHelper, _readOnlyPath, _readWritePath);
        }

        public void SetResourceHelper(IResourceHelper resourceHelper)
        {
            _resourceHelper = resourceHelper;
        }

        /// <summary>
        /// 检查资源是否存在。
        /// </summary>
        /// <param name="assetAddress">要检查资源的地址。</param>
        /// <returns>检查资源是否存在的结果。</returns>
        public HasAssetResult HasAsset(AssetAddress assetAddress)
        {
            if (!assetAddress.IsValid())
            {
                throw new GameFrameworkException("Asset address is invalid.");
            }

            AssetInfo assetInfo = GetAssetInfo(assetAddress);
            if (assetInfo == null)
            {
                return HasAssetResult.NotExist;
            }

            if (!_resourceHelper.CheckAssetAddressValid(assetAddress))
            {
                return HasAssetResult.NotExist;
            }

            if (_resourceHelper.IsNeedDownloadFromRemote(assetInfo))
            {
                return HasAssetResult.AssetOnline;
            }

            return HasAssetResult.AssetOnDisk;
        }

        public AssetInfo GetAssetInfo(AssetAddress assetAddress)
        {
            if (!assetAddress.IsValid())
            {
                throw new GameFrameworkException("Asset address is invalid.");
            }

            if (_assetInfosCache.TryGetValue(assetAddress, out AssetInfo assetInfo))
            {
                return assetInfo;
            }

            assetInfo = _resourceHelper.GetAssetInfo(assetAddress);
            _assetInfosCache[assetAddress] = assetInfo;
            return assetInfo;
        }

        public AssetInfo[] GetAssetInfos(string packageName, params string[] tags)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                throw new GameFrameworkException("Package name is invalid.");
            }

            if (tags == null || tags.Length == 0)
            {
                throw new GameFrameworkException("Tags is empty.");
            }

            var key = (packageName, tags);
            if (_assetInfosCacheByTags.TryGetValue(key, out AssetInfo[] assetInfos))
            {
                return assetInfos;
            }

            assetInfos = _resourceHelper.GetAssetInfos(packageName, tags);
            _assetInfosCacheByTags[key] = assetInfos;
            return assetInfos;
        }

        public void LoadAsset(
            AssetAddress assetAddress,
            LoadAssetCallbacks loadAssetCallbacks,
            Type assetType = null,
            int? customPriority = null,
            object userData = null)
        {
            if (!assetAddress.IsValid())
            {
                throw new GameFrameworkException("Asset address is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new GameFrameworkException("Load asset callbacks is invalid.");
            }

            _resourceLoader.LoadAsset(
                assetAddress,
                assetType,
                customPriority ?? Constant.DefaultPriority,
                loadAssetCallbacks,
                userData);
        }

        /// <summary>
        /// 卸载资源。
        /// </summary>
        /// <param name="asset">要卸载的资源。</param>
        public void UnloadAsset(object asset)
        {
            if (asset == null)
            {
                throw new GameFrameworkException("Asset is invalid.");
            }

            _resourceLoader.UnloadAsset(asset);
        }

        public void LoadScene(
            AssetAddress sceneAssetAddress,
            LoadSceneCallbacks loadSceneCallbacks,
            int? customPriority = null,
            object userData = null)
        {
            if (!sceneAssetAddress.IsValid())
            {
                throw new GameFrameworkException("Scene address is invalid.");
            }

            if (loadSceneCallbacks == null)
            {
                throw new GameFrameworkException("Load scene callbacks is invalid.");
            }

            _resourceLoader.LoadScene(
                sceneAssetAddress,
                customPriority ?? Constant.DefaultPriority,
                loadSceneCallbacks,
                userData);
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
            if (!sceneAssetAddress.IsValid())
            {
                throw new GameFrameworkException("Scene address is invalid.");
            }

            if (unloadSceneCallbacks == null)
            {
                throw new GameFrameworkException("Unload scene callbacks is invalid.");
            }

            _resourceLoader.UnloadScene(
                sceneAssetAddress,
                unloadSceneCallbacks,
                userData);
        }

        public void ClearAllCacheFiles(
            FileClearMode fileClearMode,
            ClearAllCacheFilesCallbacks clearAllCacheFilesCallbacks,
            object userData = null)
        {
            if (_resourceHelper == null)
            {
                throw new GameFrameworkException("You must set resource helper first.");
            }

            if (clearAllCacheFilesCallbacks == null)
            {
                throw new GameFrameworkException("Clear all cache files callbacks is invalid.");
            }

            string[] packageNames = _resourceHelper.GetAllPackageNames();

            if (packageNames.Length == 0)
            {
                // No packages to clear, complete immediately
                clearAllCacheFilesCallbacks.ClearAllCacheFilesComplete?.Invoke(false);
                return;
            }

            int clearingPackageCount = packageNames.Length;
            bool hasError = false;

            var clearPackageCacheFilesCallbacks = new ClearPackageCacheFilesCallbacks(
                OnClearPackageCacheFilesSuccess,
                OnClearPackageCacheFilesFailure);

            foreach (string packageName in packageNames)
            {
                ClearPackageCacheFiles(packageName, fileClearMode, clearPackageCacheFilesCallbacks, userData);
            }

            void OnClearPackageCacheFilesSuccess(string packageName)
            {
                if (clearingPackageCount <= 0)
                {
                    throw new GameFrameworkException("Clear package cache files completed more times than expected.");
                }

                clearingPackageCount--;
                clearAllCacheFilesCallbacks.ClearPackageCacheFilesSuccess?.Invoke(packageName);

                if (clearingPackageCount == 0)
                {
                    clearAllCacheFilesCallbacks.ClearAllCacheFilesComplete?.Invoke(hasError);
                }
            }

            void OnClearPackageCacheFilesFailure(string packageName, string errorMessage)
            {
                if (clearingPackageCount <= 0)
                {
                    throw new GameFrameworkException("Clear package cache files completed more times than expected.");
                }

                clearingPackageCount--;
                hasError = true;
                clearAllCacheFilesCallbacks.ClearPackageCacheFilesFailure?.Invoke(packageName, errorMessage);

                if (clearingPackageCount == 0)
                {
                    clearAllCacheFilesCallbacks.ClearAllCacheFilesComplete?.Invoke(hasError);
                }
            }
        }

        public void ClearPackageCacheFiles(
            string packageName,
            FileClearMode fileClearMode,
            ClearPackageCacheFilesCallbacks clearPackageCacheFilesCallbacks,
            object userData = null)
        {
            if (_resourceHelper == null)
            {
                throw new GameFrameworkException("You must set resource helper first.");
            }

            if (string.IsNullOrEmpty(packageName))
            {
                throw new GameFrameworkException("Package name is invalid.");
            }

            if (clearPackageCacheFilesCallbacks == null)
            {
                throw new GameFrameworkException("Clear package cache files callbacks is invalid.");
            }

            _resourceHelper.ClearPackageCacheFiles(
                packageName,
                fileClearMode,
                clearPackageCacheFilesCallbacks,
                userData);
        }
    }
}
