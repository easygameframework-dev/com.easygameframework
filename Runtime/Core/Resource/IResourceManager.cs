//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using EasyGameFramework.Core.ObjectPool;

namespace EasyGameFramework.Core.Resource
{
    /// <summary>
    /// 资源运行模式。
    /// </summary>
    public enum PlayMode
    {
        /// <summary>
        /// 编辑器下的模拟模式
        /// </summary>
        EditorSimulateMode,

        /// <summary>
        /// 离线运行模式
        /// </summary>
        OfflinePlayMode,

        /// <summary>
        /// 联机运行模式
        /// </summary>
        HostPlayMode,

        /// <summary>
        /// WebGL运行模式
        /// </summary>
        WebPlayMode,
    }

    /// <summary>
    /// 文件校验等级
    /// </summary>
    public enum FileVerifyLevel
    {
        /// <summary>
        /// 验证文件存在
        /// </summary>
        Low = 1,

        /// <summary>
        /// 验证文件大小
        /// </summary>
        Middle = 2,

        /// <summary>
        /// 验证文件大小和CRC
        /// </summary>
        High = 3,
    }

    /// <summary>
    /// 资源管理器接口。
    /// </summary>
    public interface IResourceManager
    {
        /// <summary>
        /// 获取或设置运行模式。
        /// </summary>
        PlayMode PlayMode { get; set; }

        /// <summary>
        /// 缓存系统启动时的验证级别。
        /// </summary>
        FileVerifyLevel FileVerifyLevel { get; set; }

        /// <summary>
        /// 同时下载的最大数目。
        /// </summary>
        int DownloadingMaxNum { get; set; }

        /// <summary>
        /// 失败重试最大数目。
        /// </summary>
        int FailedTryAgain { get; set; }

        /// <summary>
        /// 获取资源只读区路径。
        /// </summary>
        string ReadOnlyPath { get; }

        /// <summary>
        /// 获取资源读写区路径。
        /// </summary>
        string ReadWritePath { get; }

        /// <summary>
        /// 获取或设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）。
        /// </summary>
        long Milliseconds { get; set; }

        /// <summary>
        /// 获取或设置资源对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        float AssetAutoReleaseInterval { get; set; }

        /// <summary>
        /// 获取或设置资源对象池的容量。
        /// </summary>
        int AssetCapacity { get; set; }

        /// <summary>
        /// 获取或设置资源对象池对象过期秒数。
        /// </summary>
        float AssetExpireTime { get; set; }

        /// <summary>
        /// 获取或设置资源对象池的优先级。
        /// </summary>
        int AssetPriority { get; set; }

        /// <summary>
        /// 设置资源只读区路径。
        /// </summary>
        /// <param name="readOnlyPath">资源只读区路径。</param>
        void SetReadOnlyPath(string readOnlyPath);

        /// <summary>
        /// 设置资源读写区路径。
        /// </summary>
        /// <param name="readWritePath">资源读写区路径。</param>
        void SetReadWritePath(string readWritePath);

        /// <summary>
        /// 添加加载资源代理辅助器。
        /// </summary>
        /// <param name="loadResourceAgentHelper">要添加的加载资源代理辅助器。</param>
        void AddLoadResourceAgentHelper(ILoadResourceAgentHelper loadResourceAgentHelper);

        /// <summary>
        /// 设置资源辅助器。
        /// </summary>
        /// <param name="resourceHelper">要设置的资源辅助器。</param>
        void SetResourceHelper(IResourceHelper resourceHelper);

        /// <summary>
        /// 设置对象池管理器。
        /// </summary>
        /// <param name="objectPoolManager">对象池管理器。</param>
        void SetObjectPoolManager(IObjectPoolManager objectPoolManager);

        /// <summary>
        /// 检查资源是否存在。
        /// </summary>
        /// <param name="assetAddress">要检查资源的地址。</param>
        /// <returns>检查资源是否存在的结果。</returns>
        HasAssetResult HasAsset(AssetAddress assetAddress);

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetAddress">要获取资源信息的地址。</param>
        /// <returns></returns>
        AssetInfo GetAssetInfo(AssetAddress assetAddress);

        /// <summary>
        /// 获取资源信息数组。
        /// </summary>
        /// <param name="packageName">资源包名称。</param>
        /// <param name="tags">资源标签数组。</param>
        /// <returns>资源信息数组。</returns>
        AssetInfo[] GetAssetInfos(string packageName, params string[] tags);

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetAddress">要加载资源的地址。</param>
        /// <param name="assetType">要加载资源的类型。</param>
        /// <param name="customPriority">加载资源的优先级。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        void LoadAsset(
            AssetAddress assetAddress,
            LoadAssetCallbacks loadAssetCallbacks,
            Type assetType = null,
            int? customPriority = null,
            object userData = null);

        /// <summary>
        /// 卸载资源。
        /// </summary>
        /// <param name="asset">要卸载的资源。</param>
        void UnloadAsset(object asset);

        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetAddress">要加载场景资源的地址。</param>
        /// <param name="customPriority">加载场景资源的优先级。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        void LoadScene(
            AssetAddress sceneAssetAddress,
            LoadSceneCallbacks loadSceneCallbacks,
            int? customPriority = null,
            object userData = null);

        /// <summary>
        /// 异步卸载场景。
        /// </summary>
        /// <param name="sceneAssetAddress">要卸载场景资源的地址。</param>
        /// <param name="unloadSceneCallbacks">卸载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        void UnloadScene(AssetAddress sceneAssetAddress, UnloadSceneCallbacks unloadSceneCallbacks, object userData = null);

        /// <summary>
        /// 清理所有缓存文件。
        /// </summary>
        /// <param name="fileClearMode">文件清理模式。</param>
        /// <param name="clearAllCacheFilesCallbacks">清理所有缓存文件回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        void ClearAllCacheFiles(
            FileClearMode fileClearMode,
            ClearAllCacheFilesCallbacks clearAllCacheFilesCallbacks,
            object userData = null);

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
            object userData = null);
    }
}
