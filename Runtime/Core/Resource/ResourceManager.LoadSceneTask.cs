namespace EasyGameFramework.Core.Resource
{
    internal sealed partial class ResourceManager
    {
        private class LoadSceneTask : LoadResourceTaskBase
        {
            private LoadSceneCallbacks _loadSceneCallbacks;

            public override bool IsScene => true;

            public static LoadSceneTask Create(AssetAddress sceneAssetAddress, int priority, LoadSceneCallbacks loadSceneCallbacks, object userData)
            {
                LoadSceneTask loadSceneTask = ReferencePool.Acquire<LoadSceneTask>();
                loadSceneTask.Initialize(sceneAssetAddress, null, priority, userData);
                loadSceneTask._loadSceneCallbacks = loadSceneCallbacks;
                return loadSceneTask;
            }

            public override void Clear()
            {
                base.Clear();
                _loadSceneCallbacks = null;
            }

            public override void OnLoadAssetSuccess(LoadResourceAgent agent, AssetObject assetObject, float duration)
            {
                if (_loadSceneCallbacks.LoadSceneSuccessCallback != null)
                {
                    _loadSceneCallbacks.LoadSceneSuccessCallback(AssetAddress, assetObject.Asset, duration, UserData);
                }
            }

            public override void OnLoadAssetFailure(LoadResourceAgent agent, LoadResourceStatus status, string errorMessage)
            {
                if (_loadSceneCallbacks.LoadSceneFailureCallback != null)
                {
                    _loadSceneCallbacks.LoadSceneFailureCallback(AssetAddress, status, errorMessage, UserData);
                }
            }
        }
    }
}
