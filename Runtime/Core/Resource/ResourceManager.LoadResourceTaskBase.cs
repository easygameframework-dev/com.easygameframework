using System;

namespace EasyGameFramework.Core.Resource
{
    internal sealed partial class ResourceManager
    {
        private abstract class LoadResourceTaskBase : TaskBase
        {
            private static int s_Serial = 0;

            private AssetAddress m_AssetAddress;
            private Type m_AssetType;

            public AssetAddress Address => m_AssetAddress;
            public string PackageName => m_AssetAddress.PackageName;
            public string AssetName => m_AssetAddress.Location;
            public Type AssetType => m_AssetType;

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
                Initialize(++s_Serial, "LoadResourceTask", priority, userData);
                m_AssetAddress = assetAddress;
                m_AssetType = assetType;
            }
        }
    }
}
