using System;

namespace EasyGameFramework.Core.Resource
{
    internal sealed partial class ResourceManager
    {
        private sealed class LoadAssetTask : LoadResourceTaskBase
        {
            private LoadAssetCallbacks _loadAssetCallbacks;

            public override bool IsScene => false;

            public override void OnLoadAssetSuccess(LoadResourceAgent agent, AssetObject assetObject, float duration)
            {
                if (_loadAssetCallbacks.LoadAssetSuccessCallback != null)
                {
                    _loadAssetCallbacks.LoadAssetSuccessCallback(AssetAddress, assetObject.Asset, duration, UserData);
                }
            }

            public override void OnLoadAssetFailure(LoadResourceAgent agent, LoadResourceStatus status, string errorMessage)
            {
                if (_loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    _loadAssetCallbacks.LoadAssetFailureCallback(AssetAddress, status, errorMessage, UserData);
                }
            }

            public static LoadAssetTask Create(AssetAddress assetAddress, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks,
                object userData)
            {
                LoadAssetTask loadAssetTask = ReferencePool.Acquire<LoadAssetTask>();
                loadAssetTask.Initialize(assetAddress, assetType, priority, userData);
                loadAssetTask._loadAssetCallbacks = loadAssetCallbacks;
                return loadAssetTask;
            }

            public override void Clear()
            {
                base.Clear();
                _loadAssetCallbacks = null;
            }
        }
    }
}
