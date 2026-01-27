using System;
using System.Collections.Generic;

namespace EasyGameFramework.Core.Resource
{
    internal sealed partial class ResourceManager
    {
        private class LoadResourceAgent : ITaskAgent<LoadResourceTaskBase>
        {
            private LoadResourceTaskBase _task;
            private readonly ILoadResourceAgentHelper _loadResourceAgentHelper;
            private readonly ResourceLoader _resourceLoader;
            private readonly string _readOnlyPath;
            private readonly string _readWritePath;

            private static readonly HashSet<AssetAddress> s_loadingAssetAddresses = new HashSet<AssetAddress>();

            public LoadResourceAgent(ILoadResourceAgentHelper loadResourceAgentHelper,ResourceLoader resourceLoader, string readOnlyPath, string readWritePath)
            {
                _loadResourceAgentHelper = loadResourceAgentHelper;
                _resourceLoader = resourceLoader;
                _readOnlyPath = readOnlyPath;
                _readWritePath = readWritePath;
            }

            public LoadResourceTaskBase Task => _task;

            public void Initialize()
            {
                _loadResourceAgentHelper.LoadComplete += OnLoadResourceAgentHelperLoadComplete;
                _loadResourceAgentHelper.Error += OnLoadResourceAgentHelperError;
            }

            public void Update(float elapseSeconds, float realElapseSeconds)
            {
            }

            public void Shutdown()
            {
                _loadResourceAgentHelper.LoadComplete -= OnLoadResourceAgentHelperLoadComplete;
                _loadResourceAgentHelper.Error -= OnLoadResourceAgentHelperError;
            }

            public StartTaskStatus Start(LoadResourceTaskBase task)
            {
                if (task == null)
                {
                    throw new GameFrameworkException("Task is invalid.");
                }

                if (s_loadingAssetAddresses.Contains(task.AssetAddress))
                {
                    _task.StartTime = default(DateTime);
                    return StartTaskStatus.HasToWait;
                }

                _task = task;
                _task.StartTime = DateTime.UtcNow;

                _loadResourceAgentHelper.LoadAsset(task.AssetAddress, task.AssetType, task.IsScene, task.UserData);
                return StartTaskStatus.CanResume;
            }

            public void Reset()
            {
                _loadResourceAgentHelper.Reset();
                _task = null;
            }

            private void OnLoadResourceAgentHelperError(object sender, LoadResourceAgentHelperErrorEventArgs e)
            {
                _loadResourceAgentHelper.Reset();
                _task.OnLoadAssetFailure(this, e.Status, e.ErrorMessage);
                s_loadingAssetAddresses.Remove(_task.AssetAddress);
                _task.Done = true;
            }

            private void OnLoadResourceAgentHelperLoadComplete(object sender, LoadResourceAgentHelperLoadCompleteEventArgs e)
            {
                s_loadingAssetAddresses.Remove(_task.AssetAddress);
                _loadResourceAgentHelper.Reset();

                _resourceLoader.RegisterAsset(_task.AssetAddress, e.AssetObject);
                _task.OnLoadAssetSuccess(this, e.AssetObject, (float)(DateTime.UtcNow - _task.StartTime).TotalSeconds);
                _task.Done = true;
            }
        }
    }
}
