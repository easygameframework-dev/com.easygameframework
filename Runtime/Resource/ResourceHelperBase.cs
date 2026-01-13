//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core.Resource;
using UnityEngine;

namespace EasyGameFramework
{
    /// <summary>
    /// 资源辅助器基类。
    /// </summary>
    public abstract class ResourceHelperBase : MonoBehaviour, IResourceHelper
    {
        public abstract bool CheckAssetAddressValid(AssetAddress assetAddress);

        public abstract bool IsNeedDownloadFromRemote(AssetInfo assetInfo);

        public abstract AssetInfo GetAssetInfo(AssetAddress assetAddress);
        public abstract AssetInfo[] GetAssetInfos(string packageName, string[] tags);

        public abstract void UnloadScene(AssetAddress sceneAssetAddress, AssetObject sceneAssetObject,
            UnloadSceneCallbacks unloadSceneCallbacks, object userData);

        public abstract void UnloadAsset(AssetObject assetObject);
        public abstract string[] GetAllPackageNames();

        public abstract void ClearPackageCacheFiles(string packageName, FileClearMode fileClearMode,
            ClearPackageCacheFilesCallbacks clearPackageCacheFilesCallbacks, object userData);
    }
}
