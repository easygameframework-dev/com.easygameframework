//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core;
using EasyGameFramework.Core.Resource;
using EasyGameFramework.Core.Scene;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace EasyGameFramework
{
    /// <summary>
    /// 场景组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Scene")]
    public sealed class SceneComponent : GameFrameworkComponent
    {
        private ISceneManager _sceneManager = null;
        private EventComponent _eventComponent = null;
        private readonly Dictionary<AssetAddress, string> _sceneAssetAddressToSceneName = new Dictionary<AssetAddress, string>();
        private readonly Dictionary<AssetAddress, int> _sceneOrder = new Dictionary<AssetAddress, int>();
        private Camera _mainCamera = null;
        private Scene _gameFrameworkScene = default(Scene);

        /// <summary>
        /// 获取当前场景主摄像机。
        /// </summary>
        public Camera MainCamera
        {
            get
            {
                return _mainCamera;
            }
        }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _sceneManager = GameFrameworkEntry.GetModule<ISceneManager>();
            if (_sceneManager == null)
            {
                Log.Fatal("Scene manager is invalid.");
                return;
            }

            _sceneManager.LoadSceneSuccess += OnLoadSceneSuccess;
            _sceneManager.LoadSceneFailure += OnLoadSceneFailure;

            _sceneManager.UnloadSceneSuccess += OnUnloadSceneSuccess;
            _sceneManager.UnloadSceneFailure += OnUnloadSceneFailure;

            _gameFrameworkScene = SceneManager.GetSceneAt(GameEntry.GameFrameworkSceneId);
            if (!_gameFrameworkScene.IsValid())
            {
                Log.Fatal("Game Framework scene is invalid.");
                return;
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

            _sceneManager.SetResourceManager(GameFrameworkEntry.GetModule<IResourceManager>());
        }

        /// <summary>
        /// 获取场景名称。
        /// </summary>
        /// <param name="sceneAssetAddress">场景资源地址。</param>
        /// <returns>场景名称。</returns>
        public string GetSceneName(AssetAddress sceneAssetAddress)
        {
            // if (string.IsNullOrEmpty(sceneAssetAddress))
            // {
            //     Log.Error("Scene asset name is invalid.");
            //     return null;
            // }
            //
            // int sceneNamePosition = sceneAssetAddress.LastIndexOf('/');
            // if (sceneNamePosition + 1 >= sceneAssetAddress.Length)
            // {
            //     Log.Error("Scene asset name '{0}' is invalid.", sceneAssetAddress);
            //     return null;
            // }
            //
            // string sceneName = sceneAssetAddress.Substring(sceneNamePosition + 1);
            // sceneNamePosition = sceneName.LastIndexOf(".unity");
            // if (sceneNamePosition > 0)
            // {
            //     sceneName = sceneName.Substring(0, sceneNamePosition);
            // }
            //
            // return sceneName;

            if (_sceneAssetAddressToSceneName.TryGetValue(sceneAssetAddress, out var sceneName))
            {
                return sceneName;
            }
            Log.Error("Scene asset name '{0}' is not loaded.", sceneAssetAddress);
            return null;
        }

        /// <summary>
        /// 获取场景是否已加载。
        /// </summary>
        /// <param name="sceneAssetAddress">场景资源地址。</param>
        /// <returns>场景是否已加载。</returns>
        public bool SceneIsLoaded(AssetAddress sceneAssetAddress)
        {
            return _sceneManager.SceneIsLoaded(sceneAssetAddress);
        }

        /// <summary>
        /// 获取已加载场景的资源地址。
        /// </summary>
        /// <returns>已加载场景的资源地址。</returns>
        public AssetAddress[] GetLoadedSceneAssetAddresses()
        {
            return _sceneManager.GetLoadedSceneAssetAddresses();
        }

        /// <summary>
        /// 获取场景是否正在加载。
        /// </summary>
        /// <param name="sceneAssetAddress">场景资源地址。</param>
        /// <returns>场景是否正在加载。</returns>
        public bool SceneIsLoading(AssetAddress sceneAssetAddress)
        {
            return _sceneManager.SceneIsLoading(sceneAssetAddress);
        }

        /// <summary>
        /// 获取正在加载场景的资源地址。
        /// </summary>
        /// <returns>正在加载场景的资源地址。</returns>
        public AssetAddress[] GetLoadingSceneAssetAddresses()
        {
            return _sceneManager.GetLoadingSceneAssetAddresses();
        }

        /// <summary>
        /// 获取场景是否正在卸载。
        /// </summary>
        /// <param name="sceneAssetAddress">场景资源地址。</param>
        /// <returns>场景是否正在卸载。</returns>
        public bool SceneIsUnloading(AssetAddress sceneAssetAddress)
        {
            return _sceneManager.SceneIsUnloading(sceneAssetAddress);
        }

        /// <summary>
        /// 获取正在卸载场景的资源地址。
        /// </summary>
        /// <returns>正在卸载场景的资源地址。</returns>
        public AssetAddress[] GetUnloadingSceneAssetAddresses()
        {
            return _sceneManager.GetUnloadingSceneAssetAddresses();
        }

        /// <summary>
        /// 检查场景资源是否存在。
        /// </summary>
        /// <param name="sceneAssetAddress">要检查场景资源的名称。</param>
        /// <returns>场景资源是否存在。</returns>
        public bool HasScene(AssetAddress sceneAssetAddress)
        {
            if (!sceneAssetAddress.IsValid())
            {
                Log.Error("Scene asset name is invalid.");
                return false;
            }

            // if (!sceneAssetAddress.StartsWith("Assets/", StringComparison.Ordinal) || !sceneAssetAddress.EndsWith(".unity", StringComparison.Ordinal))
            // {
            //     Log.Error("Scene asset name '{0}' is invalid.", sceneAssetAddress);
            //     return false;
            // }

            return _sceneManager.HasScene(sceneAssetAddress);
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
                Log.Error("Scene asset name is invalid.");
                return;
            }

            // if (!sceneAssetAddress.StartsWith("Assets/", StringComparison.Ordinal) || !sceneAssetAddress.EndsWith(".unity", StringComparison.Ordinal))
            // {
            //     Log.Error("Scene asset name '{0}' is invalid.", sceneAssetAddress);
            //     return;
            // }

            _sceneManager.LoadScene(sceneAssetAddress, customPriority, userData);
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
                Log.Error("Scene asset name is invalid.");
                return;
            }

            // if (!sceneAssetAddress.StartsWith("Assets/", StringComparison.Ordinal) || !sceneAssetAddress.EndsWith(".unity", StringComparison.Ordinal))
            // {
            //     Log.Error("Scene asset name '{0}' is invalid.", sceneAssetAddress);
            //     return;
            // }

            _sceneManager.UnloadScene(sceneAssetAddress, userData);
            _sceneOrder.Remove(sceneAssetAddress);
        }

        /// <summary>
        /// 设置场景顺序。
        /// </summary>
        /// <param name="sceneAssetAddress">场景资源地址。</param>
        /// <param name="sceneOrder">要设置的场景顺序。</param>
        public void SetSceneOrder(AssetAddress sceneAssetAddress, int sceneOrder)
        {
            if (!sceneAssetAddress.IsValid())
            {
                Log.Error("Scene asset name is invalid.");
                return;
            }

            // if (!sceneAssetAddress.StartsWith("Assets/", StringComparison.Ordinal) || !sceneAssetAddress.EndsWith(".unity", StringComparison.Ordinal))
            // {
            //     Log.Error("Scene asset name '{0}' is invalid.", sceneAssetAddress);
            //     return;
            // }

            if (SceneIsLoading(sceneAssetAddress))
            {
                _sceneOrder[sceneAssetAddress] = sceneOrder;
                return;
            }

            if (SceneIsLoaded(sceneAssetAddress))
            {
                _sceneOrder[sceneAssetAddress] = sceneOrder;
                RefreshSceneOrder();
                return;
            }

            Log.Error("Scene '{0}' is not loaded or loading.", sceneAssetAddress);
        }

        /// <summary>
        /// 刷新当前场景主摄像机。
        /// </summary>
        public void RefreshMainCamera()
        {
            _mainCamera = Camera.main;
        }

        private void RefreshSceneOrder()
        {
            if (_sceneOrder.Count > 0)
            {
                AssetAddress maxSceneName = AssetAddress.Empty;
                int maxSceneOrder = 0;
                foreach (KeyValuePair<AssetAddress, int> sceneOrder in _sceneOrder)
                {
                    if (SceneIsLoading(sceneOrder.Key))
                    {
                        continue;
                    }

                    if (maxSceneName == AssetAddress.Empty)
                    {
                        maxSceneName = sceneOrder.Key;
                        maxSceneOrder = sceneOrder.Value;
                        continue;
                    }

                    if (sceneOrder.Value > maxSceneOrder)
                    {
                        maxSceneName = sceneOrder.Key;
                        maxSceneOrder = sceneOrder.Value;
                    }
                }

                if (maxSceneName == AssetAddress.Empty)
                {
                    SetActiveScene(_gameFrameworkScene);
                    return;
                }

                Scene scene = SceneManager.GetSceneByName(GetSceneName(maxSceneName));
                if (!scene.IsValid())
                {
                    Log.Error("Active scene '{0}' is invalid.", maxSceneName);
                    return;
                }

                SetActiveScene(scene);
            }
            else
            {
                SetActiveScene(_gameFrameworkScene);
            }
        }

        private void SetActiveScene(Scene activeScene)
        {
            Scene lastActiveScene = SceneManager.GetActiveScene();
            if (lastActiveScene != activeScene)
            {
                SceneManager.SetActiveScene(activeScene);
                _eventComponent.Fire(this, ActiveSceneChangedEventArgs.Create(lastActiveScene, activeScene));
            }

            RefreshMainCamera();
        }

        private void OnLoadSceneSuccess(object sender, EasyGameFramework.Core.Scene.LoadSceneSuccessEventArgs e)
        {
            _sceneAssetAddressToSceneName[e.SceneAssetAddress] = ((Scene)e.SceneAsset).name;
            if (!_sceneOrder.ContainsKey(e.SceneAssetAddress))
            {
                _sceneOrder.Add(e.SceneAssetAddress, 0);
            }

            _eventComponent.Fire(this, LoadSceneSuccessEventArgs.Create(e));
            RefreshSceneOrder();
        }

        private void OnLoadSceneFailure(object sender, EasyGameFramework.Core.Scene.LoadSceneFailureEventArgs e)
        {
            Log.Warning("Load scene failure, scene asset name '{0}', error message '{1}'.", e.SceneAssetAddress, e.ErrorMessage);
            _eventComponent.Fire(this, LoadSceneFailureEventArgs.Create(e));
        }

        private void OnUnloadSceneSuccess(object sender, EasyGameFramework.Core.Scene.UnloadSceneSuccessEventArgs e)
        {
            _sceneAssetAddressToSceneName.Remove(e.SceneAssetAddress);
            _eventComponent.Fire(this, UnloadSceneSuccessEventArgs.Create(e));
            _sceneOrder.Remove(e.SceneAssetAddress);
            RefreshSceneOrder();
        }

        private void OnUnloadSceneFailure(object sender, EasyGameFramework.Core.Scene.UnloadSceneFailureEventArgs e)
        {
            Log.Warning("Unload scene failure, scene asset name '{0}'.", e.SceneAssetAddress);
            _eventComponent.Fire(this, UnloadSceneFailureEventArgs.Create(e));
        }
    }
}
