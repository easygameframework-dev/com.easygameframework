using System;

namespace EasyGameFramework.Core.Resource
{
    internal sealed partial class ResourceManager
    {
        private abstract class LoadResourceTaskBase : TaskBase
        {
            private static int s_serial = 0;

            private AssetAddress _assetAddress;
            private Type _assetType;

            public AssetAddress AssetAddress => _assetAddress;
            public Type AssetType => _assetType;

            public DateTime StartTime { get; set; }

            public abstract bool IsScene { get; }


            public virtual void OnLoadAssetSuccess(LoadResourceAgent agent, AssetObject assetObject, float duration)
            {
            }

            public virtual void OnLoadAssetFailure(LoadResourceAgent agent, LoadResourceStatus status, string errorMessage)
            {
            }

            protected void Initialize(AssetAddress assetAddress, Type assetType, int priority, object userData)
            {
                Initialize(++s_serial, "LoadResourceTask", priority, userData);
                _assetAddress = assetAddress;
                _assetType = assetType;
            }
        }
    }
}
