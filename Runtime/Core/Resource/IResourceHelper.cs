//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace EasyGameFramework.Core.Resource
{
    /// <summary>
    /// 资源辅助器接口。
    /// </summary>
    public interface IResourceHelper
    {
        /// <summary>
        /// 检查资源地址是否有效。
        /// </summary>
        /// <param name="assetAddress">资源地址。</param>
        /// <returns>资源地址是否有效。</returns>
        bool CheckAssetAddressValid(AssetAddress assetAddress);

        /// <summary>
        /// 检查是否需要从远程下载资源。
        /// </summary>
        /// <param name="assetInfo">资源信息。</param>
        /// <returns>是否需要从远程下载资源。</returns>
        bool IsNeedDownloadFromRemote(AssetInfo assetInfo);

        /// <summary>
        /// 获取资源信息。
        /// </summary>
        /// <param name="assetAddress">资源地址。</param>
        /// <returns>资源信息。</returns>
        AssetInfo GetAssetInfo(AssetAddress assetAddress);

        /// <summary>
        /// 获取资源信息数组。
        /// </summary>
        /// <param name="packageName">资源包名称。</param>
        /// <param name="tags">资源标签数组。</param>
        /// <returns>资源信息数组。</returns>
        AssetInfo[] GetAssetInfos(string packageName, string[] tags);

        /// <summary>
        /// 卸载场景。
        /// </summary>
        /// <param name="sceneAssetAddress">场景资源地址。</param>
        /// <param name="sceneAssetObject">场景资源对象。</param>
        /// <param name="unloadSceneCallbacks">卸载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        void UnloadScene(AssetAddress sceneAssetAddress, AssetObject sceneAssetObject, UnloadSceneCallbacks unloadSceneCallbacks, object userData);

        /// <summary>
        /// 卸载资源。
        /// </summary>
        /// <param name="assetObject">要卸载的资源对象。</param>
        void UnloadAsset(AssetObject assetObject);

        /// <summary>
        /// 获取所有已初始化的资源包名称。
        /// </summary>
        /// <returns>所有已初始化的资源包名称数组。</returns>
        string[] GetAllPackageNames();

        /// <summary>
        /// 清理指定资源包的缓存文件。
        /// </summary>
        /// <param name="packageName">资源包名称。</param>
        /// <param name="fileClearMode">文件清理模式。</param>
        /// <param name="clearPackageCacheFilesCallbacks">清理资源包缓存文件回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        void ClearPackageCacheFiles(
            string packageName,
            FileClearMode fileClearMode,
            ClearPackageCacheFilesCallbacks clearPackageCacheFilesCallbacks,
            object userData);
    }
}
