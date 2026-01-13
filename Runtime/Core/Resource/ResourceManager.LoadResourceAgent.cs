using System;
using System.Collections.Generic;

namespace EasyGameFramework.Core.Resource
{
    internal sealed partial class ResourceManager
    {
        private class LoadResourceAgent : ITaskAgent<LoadResourceTaskBase>
        {
            private LoadResourceTaskBase m_Task;
            private readonly ILoadResourceAgentHelper m_LoadResourceAgentHelper;
            private readonly ResourceLoader m_ResourceLoader;
            private readonly string m_ReadOnlyPath;
            private readonly string m_ReadWritePath;

            private static readonly HashSet<string> s_LoadingAssetNames = new HashSet<string>();

            public LoadResourceAgent(ILoadResourceAgentHelper loadResourceAgentHelper,ResourceLoader resourceLoader, string readOnlyPath, string readWritePath)
            {
                m_LoadResourceAgentHelper = loadResourceAgentHelper;
                m_ResourceLoader = resourceLoader;
                m_ReadOnlyPath = readOnlyPath;
                m_ReadWritePath = readWritePath;
            }

            public LoadResourceTaskBase Task => m_Task;

            public void Initialize()
            {
                m_LoadResourceAgentHelper.LoadComplete += OnLoadResourceAgentHelperLoadComplete;
                m_LoadResourceAgentHelper.Error += OnLoadResourceAgentHelperError;
            }

            public void Update(float elapseSeconds, float realElapseSeconds)
            {
            }

            public void Shutdown()
            {
                m_LoadResourceAgentHelper.LoadComplete -= OnLoadResourceAgentHelperLoadComplete;
                m_LoadResourceAgentHelper.Error -= OnLoadResourceAgentHelperError;
            }

            public StartTaskStatus Start(LoadResourceTaskBase task)
            {
                if (task == null)
                {
                    throw new GameFrameworkException("Task is invalid.");
                }

                string key = task.Address.ToString();
                if (s_LoadingAssetNames.Contains(key))
                {
                    m_Task.StartTime = default(DateTime);
                    return StartTaskStatus.HasToWait;
                }

                m_Task = task;
                m_Task.StartTime = DateTime.UtcNow;

                m_LoadResourceAgentHelper.LoadAsset(task.Address, task.AssetType, task.IsScene, task.UserData);
                return StartTaskStatus.CanResume;
            }

            public void Reset()
            {
                m_LoadResourceAgentHelper.Reset();
                m_Task = null;
            }

            private void OnLoadResourceAgentHelperError(object sender, LoadResourceAgentHelperErrorEventArgs e)
            {
                m_LoadResourceAgentHelper.Reset();
                m_Task.OnLoadAssetFailure(this, e.Status, e.ErrorMessage);
                string key = m_Task.Address.ToString();
                s_LoadingAssetNames.Remove(key);
                m_Task.Done = true;
            }

            private void OnLoadResourceAgentHelperLoadComplete(object sender, LoadResourceAgentHelperLoadCompleteEventArgs e)
            {
                string key = m_Task.Address.ToString();
                s_LoadingAssetNames.Remove(key);
                m_LoadResourceAgentHelper.Reset();

                m_ResourceLoader.RegisterAsset(m_Task.Address, e.AssetObject);
                m_Task.OnLoadAssetSuccess(this, e.AssetObject, (float)(DateTime.UtcNow - m_Task.StartTime).TotalSeconds);
                m_Task.Done = true;
            }
        }
    }
}
