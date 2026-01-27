//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using EasyGameFramework.Core.Resource;

namespace EasyGameFramework.Core.Scene
{
    /// <summary>
    /// 场景管理器。
    /// </summary>
    internal sealed class SceneManager : GameFrameworkModule, ISceneManager
    {
        private readonly List<AssetAddress> _loadedSceneAssetAddresses;
        private readonly List<AssetAddress> _loadingSceneAssetAddresses;
        private readonly List<AssetAddress> _unloadingSceneAssetAddresses;
        private readonly LoadSceneCallbacks _loadSceneCallbacks;
        private readonly UnloadSceneCallbacks _unloadSceneCallbacks;
        private IResourceManager _resourceManager;
        private EventHandler<LoadSceneSuccessEventArgs> _loadSceneSuccessEventHandler;
        private EventHandler<LoadSceneFailureEventArgs> _loadSceneFailureEventHandler;
        private EventHandler<UnloadSceneSuccessEventArgs> _unloadSceneSuccessEventHandler;
        private EventHandler<UnloadSceneFailureEventArgs> _unloadSceneFailureEventHandler;

        /// <summary>
        /// 初始化场景管理器的新实例。
        /// </summary>
        public SceneManager()
        {
            _loadedSceneAssetAddresses = new List<AssetAddress>();
            _loadingSceneAssetAddresses = new List<AssetAddress>();
            _unloadingSceneAssetAddresses = new List<AssetAddress>();
            _loadSceneCallbacks = new LoadSceneCallbacks(LoadSceneSuccessCallback, LoadSceneFailureCallback);
            _unloadSceneCallbacks = new UnloadSceneCallbacks(UnloadSceneSuccessCallback, UnloadSceneFailureCallback);
            _resourceManager = null;
            _loadSceneSuccessEventHandler = null;
            _loadSceneFailureEventHandler = null;
            _unloadSceneSuccessEventHandler = null;
            _unloadSceneFailureEventHandler = null;
        }

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        internal override int Priority
        {
            get
            {
                return 2;
            }
        }

        /// <summary>
        /// 加载场景成功事件。
        /// </summary>
        public event EventHandler<LoadSceneSuccessEventArgs> LoadSceneSuccess
        {
            add
            {
                _loadSceneSuccessEventHandler += value;
            }
            remove
            {
                _loadSceneSuccessEventHandler -= value;
            }
        }

        /// <summary>
        /// 加载场景失败事件。
        /// </summary>
        public event EventHandler<LoadSceneFailureEventArgs> LoadSceneFailure
        {
            add
            {
                _loadSceneFailureEventHandler += value;
            }
            remove
            {
                _loadSceneFailureEventHandler -= value;
            }
        }

        /// <summary>
        /// 卸载场景成功事件。
        /// </summary>
        public event EventHandler<UnloadSceneSuccessEventArgs> UnloadSceneSuccess
        {
            add
            {
                _unloadSceneSuccessEventHandler += value;
            }
            remove
            {
                _unloadSceneSuccessEventHandler -= value;
            }
        }

        /// <summary>
        /// 卸载场景失败事件。
        /// </summary>
        public event EventHandler<UnloadSceneFailureEventArgs> UnloadSceneFailure
        {
            add
            {
                _unloadSceneFailureEventHandler += value;
            }
            remove
            {
                _unloadSceneFailureEventHandler -= value;
            }
        }

        /// <summary>
        /// 场景管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理场景管理器。
        /// </summary>
        internal override void Shutdown()
        {
            AssetAddress[] loadedSceneAssetAddresses = _loadedSceneAssetAddresses.ToArray();

            foreach (AssetAddress loadedSceneAssetAddress in loadedSceneAssetAddresses)
            {
                if (SceneIsUnloading(loadedSceneAssetAddress))
                {
                    continue;
                }

                UnloadScene(loadedSceneAssetAddress);
            }

            _loadedSceneAssetAddresses.Clear();
            _loadingSceneAssetAddresses.Clear();
            _unloadingSceneAssetAddresses.Clear();
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
        /// 获取场景是否已加载。
        /// </summary>
        /// <param name="sceneAssetAddress">场景资源地址。</param>
        /// <returns>场景是否已加载。</returns>
        public bool SceneIsLoaded(AssetAddress sceneAssetAddress)
        {
            if (!sceneAssetAddress.IsValid())
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            return _loadedSceneAssetAddresses.Contains(sceneAssetAddress);
        }

        /// <summary>
        /// 获取已加载场景的资源地址。
        /// </summary>
        /// <returns>已加载场景的资源地址。</returns>
        public AssetAddress[] GetLoadedSceneAssetAddresses()
        {
            return _loadedSceneAssetAddresses.ToArray();
        }

        /// <summary>
        /// 获取场景是否正在加载。
        /// </summary>
        /// <param name="sceneAssetAddress">场景资源地址。</param>
        /// <returns>场景是否正在加载。</returns>
        public bool SceneIsLoading(AssetAddress sceneAssetAddress)
        {
            if (!sceneAssetAddress.IsValid())
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            return _loadingSceneAssetAddresses.Contains(sceneAssetAddress);
        }

        /// <summary>
        /// 获取正在加载场景的资源地址。
        /// </summary>
        /// <returns>正在加载场景的资源地址。</returns>
        public AssetAddress[] GetLoadingSceneAssetAddresses()
        {
            return _loadingSceneAssetAddresses.ToArray();
        }

        /// <summary>
        /// 获取场景是否正在卸载。
        /// </summary>
        /// <param name="sceneAssetAddress">场景资源地址。</param>
        /// <returns>场景是否正在卸载。</returns>
        public bool SceneIsUnloading(AssetAddress sceneAssetAddress)
        {
            if (!sceneAssetAddress.IsValid())
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            return _unloadingSceneAssetAddresses.Contains(sceneAssetAddress);
        }

        /// <summary>
        /// 获取正在卸载场景的资源地址。
        /// </summary>
        /// <returns>正在卸载场景的资源地址。</returns>
        public AssetAddress[] GetUnloadingSceneAssetAddresses()
        {
            return _unloadingSceneAssetAddresses.ToArray();
        }

        /// <summary>
        /// 检查场景资源是否存在。
        /// </summary>
        /// <param name="sceneAssetAddress">要检查场景资源的地址。</param>
        /// <returns>场景资源是否存在。</returns>
        public bool HasScene(AssetAddress sceneAssetAddress)
        {
            if (!sceneAssetAddress.IsValid())
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            return _resourceManager.HasAsset(sceneAssetAddress) != HasAssetResult.NotExist;
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneAssetAddress">场景资源地址。</param>
        /// <param name="customPriority">加载场景资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadScene(AssetAddress sceneAssetAddress, int? customPriority = null, object userData = null)
        {
            if (!sceneAssetAddress.IsValid())
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            if (_resourceManager == null)
            {
                throw new GameFrameworkException("You must set resource manager first.");
            }

            if (SceneIsUnloading(sceneAssetAddress))
            {
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is being unloaded.", sceneAssetAddress));
            }

            if (SceneIsLoading(sceneAssetAddress))
            {
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is being loaded.", sceneAssetAddress));
            }

            if (SceneIsLoaded(sceneAssetAddress))
            {
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is already loaded.", sceneAssetAddress));
            }

            _loadingSceneAssetAddresses.Add(sceneAssetAddress);
            _resourceManager.LoadScene(sceneAssetAddress, _loadSceneCallbacks, customPriority, userData);
        }

        /// <summary>
        /// 卸载场景。
        /// </summary>
        /// <param name="sceneAssetAddress">场景资源地址。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void UnloadScene(AssetAddress sceneAssetAddress, object userData = null)
        {
            if (!sceneAssetAddress.IsValid())
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            if (_resourceManager == null)
            {
                throw new GameFrameworkException("You must set resource manager first.");
            }

            if (SceneIsUnloading(sceneAssetAddress))
            {
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is being unloaded.", sceneAssetAddress));
            }

            if (SceneIsLoading(sceneAssetAddress))
            {
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is being loaded.", sceneAssetAddress));
            }

            if (!SceneIsLoaded(sceneAssetAddress))
            {
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is not loaded yet.", sceneAssetAddress));
            }

            _unloadingSceneAssetAddresses.Add(sceneAssetAddress);
            _resourceManager.UnloadScene(sceneAssetAddress, _unloadSceneCallbacks, userData);
        }

        private void LoadSceneSuccessCallback(AssetAddress sceneAssetAddress, object sceneAsset, float duration, object userData)
        {
            _loadingSceneAssetAddresses.Remove(sceneAssetAddress);
            _loadedSceneAssetAddresses.Add(sceneAssetAddress);
            if (_loadSceneSuccessEventHandler != null)
            {
                LoadSceneSuccessEventArgs loadSceneSuccessEventArgs = LoadSceneSuccessEventArgs.Create(sceneAssetAddress, sceneAsset, duration, userData);
                _loadSceneSuccessEventHandler(this, loadSceneSuccessEventArgs);
                ReferencePool.Release(loadSceneSuccessEventArgs);
            }
        }

        private void LoadSceneFailureCallback(AssetAddress sceneAssetAddress, LoadResourceStatus status, string errorMessage, object userData)
        {
            _loadingSceneAssetAddresses.Remove(sceneAssetAddress);
            string appendErrorMessage = Utility.Text.Format("Load scene failure, scene asset name '{0}', status '{1}', error message '{2}'.", sceneAssetAddress, status, errorMessage);
            if (_loadSceneFailureEventHandler != null)
            {
                LoadSceneFailureEventArgs loadSceneFailureEventArgs = LoadSceneFailureEventArgs.Create(sceneAssetAddress, appendErrorMessage, userData);
                _loadSceneFailureEventHandler(this, loadSceneFailureEventArgs);
                ReferencePool.Release(loadSceneFailureEventArgs);
                return;
            }

            throw new GameFrameworkException(appendErrorMessage);
        }

        private void UnloadSceneSuccessCallback(AssetAddress sceneAssetAddress, object userData)
        {
            _unloadingSceneAssetAddresses.Remove(sceneAssetAddress);
            _loadedSceneAssetAddresses.Remove(sceneAssetAddress);
            if (_unloadSceneSuccessEventHandler != null)
            {
                UnloadSceneSuccessEventArgs unloadSceneSuccessEventArgs = UnloadSceneSuccessEventArgs.Create(sceneAssetAddress, userData);
                _unloadSceneSuccessEventHandler(this, unloadSceneSuccessEventArgs);
                ReferencePool.Release(unloadSceneSuccessEventArgs);
            }
        }

        private void UnloadSceneFailureCallback(AssetAddress sceneAssetAddress, string errorMessage, object userData)
        {
            _unloadingSceneAssetAddresses.Remove(sceneAssetAddress);
            if (_unloadSceneFailureEventHandler != null)
            {
                UnloadSceneFailureEventArgs unloadSceneFailureEventArgs = UnloadSceneFailureEventArgs.Create(sceneAssetAddress, errorMessage, userData);
                _unloadSceneFailureEventHandler(this, unloadSceneFailureEventArgs);
                ReferencePool.Release(unloadSceneFailureEventArgs);
                return;
            }

            throw new GameFrameworkException(Utility.Text.Format("Unload scene failure, scene asset name '{0}'.", sceneAssetAddress));
        }
    }
}
